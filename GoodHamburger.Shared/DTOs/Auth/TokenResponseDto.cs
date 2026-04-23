namespace GoodHamburger.Shared.DTOs.Auth;

public sealed record TokenResponseDto(
    string Token,
    string RefreshToken,
    string Email,
    string Nome,
    IEnumerable<string> Roles,
    DateTime Expiracao);
