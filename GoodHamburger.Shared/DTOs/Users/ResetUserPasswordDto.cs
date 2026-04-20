namespace GoodHamburger.Shared.DTOs.Users;

public sealed record ResetUserPasswordDto(string Email, string Token, string NewPassword);
