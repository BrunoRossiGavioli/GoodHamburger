using GoodHamburger.API.Entities.PurchaseOrders;
using GoodHamburger.API.Extensions.Entities;
using GoodHamburger.API.Repositories.Customers;
using GoodHamburger.API.Repositories.PurchaseOrders;
using GoodHamburger.Shared.DTOs.PurchaseOrders;
using GoodHamburger.Shared.Models.PurchaseOrders;

namespace GoodHamburger.API.Services.PurchaseOrdens;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;

    public OrderService(IOrderRepository orderRepository, ICustomerRepository customerRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    public async Task<IEnumerable<Order>> FindAsync(FindOrderDto dto)
    {
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
        if (dto.CustomerId.HasValue)
        {
            var customerExists = await _customerRepository.ExistsAsync(c => c.Id == dto.CustomerId.Value);
            if (!customerExists)
                throw new InvalidOperationException("Customer not found.");

            orderEntity.CustomerId = dto.CustomerId;
        }
        else
        {
            orderEntity.CustomerName = dto.CustomerName.Trim();
            orderEntity.CustomerPhone = dto.CustomerPhone.Trim();
            orderEntity.CustomerAddress = dto.CustomerAddress.Trim();
        }

        orderEntity.CustomerId = dto.CustomerId;
        orderEntity.OrderDate = DateTime.UtcNow;
        orderEntity.Subtotal = dto.Subtotal;
        orderEntity.Discount = dto.Discount;
        orderEntity.Total = dto.Total;

        orderEntity.Items = [.. dto.items.Select(i => new OrderItemEntity
        {
            OrderId = orderEntity.Id,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Observation = i.Observation.Trim()
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
