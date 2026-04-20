namespace GoodHamburger.Shared.DTOs.Products;

public sealed record CreateProductDto(string Name, string Description, string Type, decimal Price);
