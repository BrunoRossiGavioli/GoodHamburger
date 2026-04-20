using GoodHamburger.Shared.Models.Customers;
using GoodHamburger.Shared.Models.PurchaseOrders;

namespace GoodHamburger.Shared.Extensions.Models
{
    public static class OrderExtension
    {
        public static Order FromCustomer(Guid id, DateTime orderDate, decimal subtotal, decimal discount, decimal total, Customer customer, IReadOnlyCollection<OrderItem> items)
        {
            ArgumentNullException.ThrowIfNull(customer);
            return new(id, orderDate, subtotal, discount, total, customer.Id, customer.Name, customer.Phone, customer.Address, items);
        }
    }
}
