using GoodHamburger.API.Entities.PurshOrders;

namespace GoodHamburger.API.Entities.Customers
{
    public class CustomerEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Address { get; set; } = default!;

        public ICollection<OrderEntity> Orders { get; set; } = default!;
    }
}
