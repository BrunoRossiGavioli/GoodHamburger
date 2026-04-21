using GoodHamburger.Shared.Enums;

namespace GoodHamburger.Shared.DTOs.Products;

public sealed record CreateProductDto(string Name, string Description, ProductType Type, decimal Price);
