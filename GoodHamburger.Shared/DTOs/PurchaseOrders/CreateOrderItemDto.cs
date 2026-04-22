namespace GoodHamburger.Shared.DTOs.PurchaseOrders;

public record CreateOrderItemDto(Guid ProductId, int Quantity, string Observation)
{
}
