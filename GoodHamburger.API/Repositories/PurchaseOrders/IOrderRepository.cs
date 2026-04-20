using GoodHamburger.API.Entities.PurchaseOrders;

namespace GoodHamburger.API.Repositories.PurchaseOrders;

public interface IOrderRepository : IRepository<OrderEntity, Guid>
{
}
