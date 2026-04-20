namespace GoodHamburger.Shared.DTOs.Products;

public sealed record FindProductPriceDto(Guid? ProductId = null, DateTime? StartDate = null);
