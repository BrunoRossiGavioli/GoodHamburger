using GoodHamburger.API.Entities.Customers;

namespace GoodHamburger.API.Entities.PurchaseOrders
{
    public class OrderEntity
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }

        public DateTime OrderDate { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        public CustomerEntity? Customer { get; set; }
        public ICollection<OrderItemEntity> Items { get; set; } = default!;
    }
}
