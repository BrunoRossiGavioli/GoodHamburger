namespace GoodHamburger.Shared.DTOs.Users;

public sealed record GetUserDto(
    Guid Id,
    string Name,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    IEnumerable<string> Roles);
