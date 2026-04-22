namespace GoodHamburger.Shared.DTOs.PurchaseOrders;

public sealed record CreateOrderDto(
    Guid? CustomerId,
    string CustomerName,
    string CustomerPhone,
    string CustomerAddress,
    IEnumerable<CreateOrderItemDto> items);
