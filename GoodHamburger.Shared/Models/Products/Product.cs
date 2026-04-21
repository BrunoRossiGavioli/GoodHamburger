using GoodHamburger.Shared.Enums;

namespace GoodHamburger.Shared.Models.Products;

public sealed record Product(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    ProductType Type);
