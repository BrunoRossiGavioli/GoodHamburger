namespace GoodHamburger.Shared.DTOs.Users;

public sealed record ChangeUserPasswordDto(Guid Id, string NewPassword);
