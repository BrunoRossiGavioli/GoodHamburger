using GoodHamburger.Shared.DTOs.PurchaseOrders;
using GoodHamburger.Shared.Models.PurchaseOrders;

namespace GoodHamburger.API.Services.PurchaseOrdens;

//TODO: Implementar o serviço de ordens de compra
public class OrderService : IOrderService
{
    public Task<Order> CreateAsync(CreateOrderDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Order>> FindAsync(FindOrderDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Order>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Order?> GetAsync(GetOrderDto dto)
    {
        throw new NotImplementedException();
    }

    public Task UpdateActiveState(UpdateActiveStateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<Order> UpdateAsync(UpdateOrderDto dto)
    {
        throw new NotImplementedException();
    }
}
