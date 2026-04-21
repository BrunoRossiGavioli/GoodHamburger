namespace GoodHamburger.Shared.DTOs.Products;

public sealed record FindProductPriceDto(Guid ProductId, DateTime? StartDate = null);
