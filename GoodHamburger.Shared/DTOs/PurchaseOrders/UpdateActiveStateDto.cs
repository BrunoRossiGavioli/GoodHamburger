namespace GoodHamburger.Shared.DTOs.PurchaseOrders;

public sealed record UpdateActiveStateDto(Guid Id, bool IsActive);
