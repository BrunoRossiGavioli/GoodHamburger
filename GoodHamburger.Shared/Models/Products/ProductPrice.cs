namespace GoodHamburger.Shared.Models.Products;

public sealed record ProductPrice(
    Guid Id,
    Guid ProductId,
    decimal Value,
    DateTime StartDate,
    DateTime? EndDate,
    string Reason);
