using GoodHamburger.API.Entities.PurchaseOrders;

namespace GoodHamburger.API.Repositories.PurchaseOrders
{
    public interface IOrderItemRepository : IRepository<OrderItemEntity, Guid>
    {
    }
}
