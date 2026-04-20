namespace GoodHamburger.Shared.Models.Customers;

public sealed record Customer(
    Guid Id,
    string Name,
    string Phone,
    string Address);
