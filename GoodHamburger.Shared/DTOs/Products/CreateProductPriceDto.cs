namespace GoodHamburger.Shared.DTOs.Products;

public sealed record CreateProductPriceDto(Guid ProductId, decimal Value, DateTime StartDate, DateTime? EndDate, string Reason);
