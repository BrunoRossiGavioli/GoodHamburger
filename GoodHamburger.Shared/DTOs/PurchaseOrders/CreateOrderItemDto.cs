namespace GoodHamburger.Shared.DTOs.PurchaseOrders;

public record CreateOrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice, string Observation)
{
}
