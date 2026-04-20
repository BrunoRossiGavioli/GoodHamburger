namespace GoodHamburger.Shared.DTOs.Users;

public sealed record CreateUserDto(string Name, string Email, string Password, string Role);
