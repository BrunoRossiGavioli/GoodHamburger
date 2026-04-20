namespace GoodHamburger.Shared.DTOs.PurchaseOrders;

public sealed record UpdateOrderActiveStateDto(Guid Id, bool IsActive);
