namespace GoodHamburger.Shared.DTOs.Products;

public sealed record UpdateProductPriceDto(Guid Id, Guid ProductId, decimal Value, DateTime StartDate, DateTime? EndDate, string Reason);
