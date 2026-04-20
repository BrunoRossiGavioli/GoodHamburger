using GoodHamburger.Shared.DTOs.PurchaseOrders;
using GoodHamburger.Shared.Models.PurchaseOrders;

namespace GoodHamburger.API.Services.PurchaseOrdens;

public interface IOrderService
{
    Task<Order?> GetAsync(GetOrderDto dto);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> FindAsync(FindOrderDto dto);

    Task<Order> CreateAsync(CreateOrderDto dto);
    Task<Order> UpdateAsync(UpdateOrderDto dto);
    Task UpdateActiveState(UpdateActiveStateDto dto);
}
