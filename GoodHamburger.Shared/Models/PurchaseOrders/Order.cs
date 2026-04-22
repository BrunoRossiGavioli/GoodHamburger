using GoodHamburger.Shared.Enums;

namespace GoodHamburger.Shared.Models.PurchaseOrders;

public sealed record Order(
    Guid Id,
    DateTime OrderDate,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    Guid? CustomerId,
    string CustomerName,
    string CustomerPhone,
    string CustomerAddress,
    OrderStatus Status,
    IReadOnlyCollection<OrderItem> Items
);
