namespace GoodHamburger.Shared.DTOs.PurchaseOrders;

public sealed record FindOrderDto(Guid? CustomerId = null, DateTime? OrderDate = null);
