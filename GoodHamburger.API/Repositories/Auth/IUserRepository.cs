using GoodHamburger.API.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace GoodHamburger.API.Repositories.Auth;

public interface IUserRepository
{
    Task<UserEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserEntity?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserEntity>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<IdentityResult> CriarAsync(UserEntity usuario, string senha);
    Task<IdentityResult> AtualizarAsync(UserEntity usuario);
    Task<IdentityResult> AdicionarRoleAsync(UserEntity usuario, string role);
    Task<IdentityResult> RemoverRoleAsync(UserEntity usuario, string role);
    Task<IList<string>> ObterRolesAsync(UserEntity usuario);
    Task<bool> VerificarSenhaAsync(UserEntity usuario, string senha);
    Task<string> GerarTokenRedefinicaoSenhaAsync(UserEntity usuario);
    Task<IdentityResult> RedefinirSenhaAsync(UserEntity usuario, string token, string novaSenha);
    Task<IdentityResult> AlterarSenhaAdminAsync(UserEntity usuario, string novaSenha);
}
