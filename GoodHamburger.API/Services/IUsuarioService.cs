using GoodHamburger.Shared.DTOs.Usuario;

namespace GoodHamburger.API.Services;

public interface IUsuarioService
{
    Task<UsuarioResponseDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioResponseDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<UsuarioResponseDto> CriarAsync(CriarUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<UsuarioResponseDto> AlterarAsync(Guid id, AlterarUsuarioDto dto, CancellationToken cancellationToken = default);
    Task AlterarSenhaAdminAsync(Guid id, AlterarSenhaAdminDto dto, CancellationToken cancellationToken = default);
    Task InativarAsync(Guid id, CancellationToken cancellationToken = default);
    Task RedefinirSenhaAsync(RedefinirSenhaDto dto, CancellationToken cancellationToken = default);
}
