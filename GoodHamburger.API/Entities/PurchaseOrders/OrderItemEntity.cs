using GoodHamburger.API.Entities.Products;
using GoodHamburger.API.Entities.PurshOrders;

namespace GoodHamburger.API.Entities.PurchaseOrders
{
    public class OrderItemEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Observation { get; set; } = default!;

        public OrderEntity Order { get; set; } = default!;
        public ProductEntity Product { get; set; } = default!;
    }
}
