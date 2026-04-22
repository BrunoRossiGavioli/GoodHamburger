using GoodHamburger.Shared.Extensions.Models;
using GoodHamburger.Shared.Models.Customers;
using GoodHamburger.Shared.Models.PurchaseOrders;

namespace GoodHamburger.Tests.Extensions;

public class OrderExtensionTests
{
    [Fact]
    public void FromCustomer_NullCustomer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            OrderExtension.FromCustomer(Guid.NewGuid(), DateTime.UtcNow, 5m, 0m, 5m, null!, []));
    }

    [Fact]
    public void FromCustomer_ValidArgs_CreatesOrderWithCustomerData()
    {
        var id = Guid.NewGuid();
        var orderDate = new DateTime(2025, 6, 1);
        var customer = new Customer(Guid.NewGuid(), "Ana", "11977777777", "Rua D, 4");

        var order = OrderExtension.FromCustomer(id, orderDate, 7.00m, 0.70m, 6.30m, customer, []);

        Assert.Equal(id, order.Id);
        Assert.Equal(orderDate, order.OrderDate);
        Assert.Equal(7.00m, order.Subtotal);
        Assert.Equal(0.70m, order.Discount);
        Assert.Equal(6.30m, order.Total);
        Assert.Equal(customer.Id, order.CustomerId);
        Assert.Equal("Ana", order.CustomerName);
        Assert.Equal("11977777777", order.CustomerPhone);
        Assert.Equal("Rua D, 4", order.CustomerAddress);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void FromCustomer_WithItems_IncludesItemsInOrder()
    {
        var customer = new Customer(Guid.NewGuid(), "Carlos", "11966666666", "Rua E");
        IReadOnlyCollection<OrderItem> items =
        [
            new OrderItem(2, "Sem sal", null!)
        ];

        var order = OrderExtension.FromCustomer(Guid.NewGuid(), DateTime.UtcNow, 10m, 0m, 10m, customer, items);

        Assert.Single(order.Items);
        Assert.Equal(2, order.Items.First().Quantity);
    }
}
