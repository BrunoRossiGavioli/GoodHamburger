namespace GoodHamburger.Shared.DTOs.PurchaseOrders;

public sealed record UpdateOrderDto(Guid Id, Guid? CustomerId, string CustomerName, string CustomerPhone, string CustomerAddress, decimal Subtotal, decimal Discount, decimal Total);
