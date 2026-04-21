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
        if (orderEntity is null)
            return null;

        return orderEntity.MapEntityToModel();
    }

    public async Task<Order> CreateAsync(CreateOrderDto dto)
    {
        if (dto.CustomerId.HasValue)
        {
            var customerExists = await _customerRepository.ExistsAsync(c => c.Id == dto.CustomerId.Value);
            if (!customerExists)
                throw new InvalidOperationException("Customer not found.");
        }

        var orderEntity = await _orderRepository.AddAsync(new OrderEntity
        {
            CustomerId = dto.CustomerId,
            OrderDate = DateTime.UtcNow,
            CustomerName = dto.CustomerName.Trim(),
            CustomerPhone = dto.CustomerPhone.Trim(),
            CustomerAddress = dto.CustomerAddress.Trim(),
            Subtotal = dto.Subtotal,
            Discount = dto.Discount,
            Total = dto.Total,
        });
        await _orderRepository.SaveChangesAsync();
        return orderEntity.MapEntityToModel();
    }

    public async Task<Order> UpdateAsync(UpdateOrderDto dto)
    {
        if (dto.CustomerId.HasValue)
        {
            var customerExists = await _customerRepository.ExistsAsync(c => c.Id == dto.CustomerId.Value);
            if (!customerExists)
                throw new InvalidOperationException("Customer not found.");
        }

        await _orderRepository.UpdateAsync(new OrderEntity
        {
            Id = dto.Id,
            CustomerId = dto.CustomerId,
            OrderDate = DateTime.UtcNow,
            CustomerName = dto.CustomerName.Trim(),
            CustomerPhone = dto.CustomerPhone.Trim(),
            CustomerAddress = dto.CustomerAddress.Trim(),
            Subtotal = dto.Subtotal,
            Discount = dto.Discount,
            Total = dto.Total,
        });
        await _orderRepository.SaveChangesAsync();

        return await GetAsync(new GetOrderDto(dto.Id)) ?? throw new InvalidOperationException("Failed to retrieve the updated order.");
    }

    public async Task UpdateActiveState(UpdateOrderActiveStateDto dto)
    {
        //TODO: UpdateOrderActiveState
        throw new NotImplementedException();
    }


}
