using GoodHamburger.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.API.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly UserManager<UsuarioEntity> _userManager;

    public UsuarioRepository(UserManager<UsuarioEntity> userManager)
    {
        _userManager = userManager;
    }

    public Task<UsuarioEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _userManager.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<UsuarioEntity?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _userManager.FindByEmailAsync(email);

    public async Task<IReadOnlyList<UsuarioEntity>> ObterTodosAsync(CancellationToken cancellationToken = default) =>
        await _userManager.Users.ToListAsync(cancellationToken);

    public Task<IdentityResult> CriarAsync(UsuarioEntity usuario, string senha) =>
        _userManager.CreateAsync(usuario, senha);

    public Task<IdentityResult> AtualizarAsync(UsuarioEntity usuario) =>
        _userManager.UpdateAsync(usuario);

    public Task<IdentityResult> AdicionarRoleAsync(UsuarioEntity usuario, string role) =>
        _userManager.AddToRoleAsync(usuario, role);

    public Task<IdentityResult> RemoverRoleAsync(UsuarioEntity usuario, string role) =>
        _userManager.RemoveFromRoleAsync(usuario, role);

    public Task<IList<string>> ObterRolesAsync(UsuarioEntity usuario) =>
        _userManager.GetRolesAsync(usuario);

    public Task<bool> VerificarSenhaAsync(UsuarioEntity usuario, string senha) =>
        _userManager.CheckPasswordAsync(usuario, senha);

    public Task<string> GerarTokenRedefinicaoSenhaAsync(UsuarioEntity usuario) =>
        _userManager.GeneratePasswordResetTokenAsync(usuario);

    public Task<IdentityResult> RedefinirSenhaAsync(UsuarioEntity usuario, string token, string novaSenha) =>
        _userManager.ResetPasswordAsync(usuario, token, novaSenha);

    public async Task<IdentityResult> AlterarSenhaAdminAsync(UsuarioEntity usuario, string novaSenha)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        return await _userManager.ResetPasswordAsync(usuario, token, novaSenha);
    }
}
