namespace GoodHamburger.Shared.DTOs.Users;

public sealed record ResetPasswordDto(string Email, string Token, string NewPassword);
