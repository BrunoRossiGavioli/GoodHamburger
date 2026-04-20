namespace GoodHamburger.Shared.DTOs.Products;

public sealed record UpdateProductActiveStateDto(Guid Id, bool IsActive);
