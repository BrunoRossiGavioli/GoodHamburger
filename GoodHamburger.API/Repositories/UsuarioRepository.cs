using GoodHamburger.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.API.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly UserManager<UserEntity> _userManager;

    public UsuarioRepository(UserManager<UserEntity> userManager)
    {
        _userManager = userManager;
    }

    public Task<UserEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _userManager.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<UserEntity?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _userManager.FindByEmailAsync(email);

    public async Task<IReadOnlyList<UserEntity>> ObterTodosAsync(CancellationToken cancellationToken = default) =>
        await _userManager.Users.ToListAsync(cancellationToken);

    public Task<IdentityResult> CriarAsync(UserEntity usuario, string senha) =>
        _userManager.CreateAsync(usuario, senha);

    public Task<IdentityResult> AtualizarAsync(UserEntity usuario) =>
        _userManager.UpdateAsync(usuario);

    public Task<IdentityResult> AdicionarRoleAsync(UserEntity usuario, string role) =>
        _userManager.AddToRoleAsync(usuario, role);

    public Task<IdentityResult> RemoverRoleAsync(UserEntity usuario, string role) =>
        _userManager.RemoveFromRoleAsync(usuario, role);

    public Task<IList<string>> ObterRolesAsync(UserEntity usuario) =>
        _userManager.GetRolesAsync(usuario);

    public Task<bool> VerificarSenhaAsync(UserEntity usuario, string senha) =>
        _userManager.CheckPasswordAsync(usuario, senha);

    public Task<string> GerarTokenRedefinicaoSenhaAsync(UserEntity usuario) =>
        _userManager.GeneratePasswordResetTokenAsync(usuario);

    public Task<IdentityResult> RedefinirSenhaAsync(UserEntity usuario, string token, string novaSenha) =>
        _userManager.ResetPasswordAsync(usuario, token, novaSenha);

    public async Task<IdentityResult> AlterarSenhaAdminAsync(UserEntity usuario, string novaSenha)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        return await _userManager.ResetPasswordAsync(usuario, token, novaSenha);
    }
}
