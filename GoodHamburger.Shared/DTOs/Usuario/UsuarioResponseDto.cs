namespace GoodHamburger.Shared.DTOs.Usuario;

public sealed record UsuarioResponseDto(
    Guid Id,
    string Nome,
    string Email,
    bool Ativo,
    DateTime CriadoEm,
    IEnumerable<string> Roles);
