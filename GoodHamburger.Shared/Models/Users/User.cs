namespace GoodHamburger.Shared.Models.Users;

public sealed record User(
    Guid Id,
    string Name,
    string Email,
    bool IsActive,
    DateTime CreateAt,
    IEnumerable<string> Roles);
