using GoodHamburger.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace GoodHamburger.API.Repositories;

public interface IUsuarioRepository
{
    Task<UsuarioEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UsuarioEntity?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioEntity>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<IdentityResult> CriarAsync(UsuarioEntity usuario, string senha);
    Task<IdentityResult> AtualizarAsync(UsuarioEntity usuario);
    Task<IdentityResult> AdicionarRoleAsync(UsuarioEntity usuario, string role);
    Task<IdentityResult> RemoverRoleAsync(UsuarioEntity usuario, string role);
    Task<IList<string>> ObterRolesAsync(UsuarioEntity usuario);
    Task<bool> VerificarSenhaAsync(UsuarioEntity usuario, string senha);
    Task<string> GerarTokenRedefinicaoSenhaAsync(UsuarioEntity usuario);
    Task<IdentityResult> RedefinirSenhaAsync(UsuarioEntity usuario, string token, string novaSenha);
    Task<IdentityResult> AlterarSenhaAdminAsync(UsuarioEntity usuario, string novaSenha);
}
