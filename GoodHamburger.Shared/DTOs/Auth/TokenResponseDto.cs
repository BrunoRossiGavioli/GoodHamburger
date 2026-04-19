namespace GoodHamburger.Shared.DTOs.Auth;

public sealed record TokenResponseDto(
    string Token,
    string Email,
    string Nome,
    IEnumerable<string> Roles,
    DateTime Expiracao);
