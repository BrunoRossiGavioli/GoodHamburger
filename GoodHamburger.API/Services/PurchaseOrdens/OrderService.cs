using GoodHamburger.API.Entities.Customers;
using GoodHamburger.API.Entities.PurchaseOrders;
using GoodHamburger.API.Extensions.Entities;
using GoodHamburger.API.Repositories.Customers;
using GoodHamburger.API.Repositories.Products;
using GoodHamburger.API.Repositories.PurchaseOrders;
using GoodHamburger.Shared.DTOs.PurchaseOrders;
using GoodHamburger.Shared.Extensions.Models;
using GoodHamburger.Shared.Models.PurchaseOrders;

namespace GoodHamburger.API.Services.PurchaseOrdens;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(IOrderRepository orderRepository, ICustomerRepository customerRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<IEnumerable<Order>> FindAsync(FindOrderDto dto)
    {
        if(dto.CustomerId is null && dto.OrderDate is null)
            return await GetAllAsync();

        var orders = await _orderRepository.FindAsync(e =>
            (dto.CustomerId == null || e.CustomerId == dto.CustomerId) &&
            (dto.OrderDate == null || e.OrderDate.Date == dto.OrderDate.Value.Date));

        return orders.Select(o => o.MapEntityToModel());
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return (await _orderRepository.GetAllAsync()).Select(o => o.MapEntityToModel());
    }

    public async Task<Order?> GetAsync(GetOrderDto dto)
    {
        var orderEntity = await _orderRepository.GetByIdAsync(dto.Id);
        return orderEntity?.MapEntityToModel();
    }

    public async Task<Order> CreateAsync(CreateOrderDto dto)
    {
        var orderEntity = new OrderEntity();

        // Resolve customer
        if (dto.CustomerId.HasValue)
        {
            var customerEntity = await _customerRepository.GetByIdAsync(dto.CustomerId.Value) ?? throw new InvalidOperationException("Customer not found.");
            orderEntity.CustomerId = dto.CustomerId;
            orderEntity.Customer = customerEntity;
        }
        else
        {
            orderEntity.CustomerName = dto.CustomerName.Trim();
            orderEntity.CustomerPhone = dto.CustomerPhone.Trim();
            orderEntity.CustomerAddress = dto.CustomerAddress.Trim();
        }

        // Load and validate products
        var productIds = dto.items.Select(i => i.ProductId).ToList();
        var products = (await _productRepository.FindAsync(p => productIds.Contains(p.Id))).ToList();

        var missingIds = productIds.Except(products.Select(p => p.Id)).ToList();
        if (missingIds.Count != 0)
            throw new InvalidOperationException($"Produto(s) não encontrado(s): {string.Join(", ", missingIds)}");

        // Build order item models for validation and calculation
        var orderItems = dto.items
            .Select(i => new OrderItem(
                i.Quantity,
                i.UnitPrice,
                i.Observation?.Trim() ?? string.Empty,
                products.First(p => p.Id == i.ProductId).MapEntityToModel()))
            .ToList();

        // Apply business rules — throws OrderException if invalid
        orderItems.ThrowIfInvalidOrder();

        // Calculate financials server-side
        var (subtotal, discount, total) = orderItems.CalculateSubtotalAndDiscount();

        // Persist entity
        orderEntity.OrderDate = DateTime.UtcNow;
        orderEntity.Subtotal = subtotal;
        orderEntity.Discount = discount;
        orderEntity.Total = total;
        orderEntity.Items = [.. dto.items.Select(i => new OrderItemEntity
        {
            OrderId = orderEntity.Id,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Observation = i.Observation?.Trim() ?? string.Empty,
            Product = products.First(p => p.Id == i.ProductId)
        })];

        await _orderRepository.AddAsync(orderEntity);
        await _orderRepository.SaveChangesAsync();

        return orderEntity.MapEntityToModel();
    }

    public async Task UpdateStatus(UpdateOrderStatusDto dto)
    {
        var orderEntity = await _orderRepository.GetByIdAsync(dto.Id) ?? throw new InvalidOperationException("Order not found.");
        orderEntity.Status = dto.Status;

        await _orderRepository.SaveChangesAsync();
    }
}
