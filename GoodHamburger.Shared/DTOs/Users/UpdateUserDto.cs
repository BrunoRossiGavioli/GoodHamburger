namespace GoodHamburger.Shared.DTOs.Users;

public sealed record UpdateUserDto(Guid Id, string Name, string Email, string Role);
