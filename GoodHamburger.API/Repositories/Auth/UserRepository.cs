using GoodHamburger.API.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.API.Repositories.Auth;

public class UserRepository : IUserRepository
{
    private readonly UserManager<UserEntity> _userManager;

    public UserRepository(UserManager<UserEntity> userManager)
    {
        _userManager = userManager;
    }

    public Task<UserEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        _userManager.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _userManager.FindByEmailAsync(email);

    public async Task<IReadOnlyList<UserEntity>> GetAll(CancellationToken cancellationToken = default) =>
        await _userManager.Users.ToListAsync(cancellationToken);

    public Task<IdentityResult> CreateAsync(UserEntity user, string password) =>
        _userManager.CreateAsync(user, password);

    public Task<IdentityResult> UpdateAsync(UserEntity user) =>
        _userManager.UpdateAsync(user);

    public Task<IdentityResult> AddRole(UserEntity user, string role) =>
        _userManager.AddToRoleAsync(user, role);

    public Task<IdentityResult> RemoveRole(UserEntity user, string role) =>
        _userManager.RemoveFromRoleAsync(user, role);

    public Task<IList<string>> GetAllRoles(UserEntity user) =>
        _userManager.GetRolesAsync(user);

    public Task<bool> VerifyPasswordAsync(UserEntity user, string password) =>
        _userManager.CheckPasswordAsync(user, password);

    public Task<string> GenerateTokenPasswordResetAsync(UserEntity user) =>
        _userManager.GeneratePasswordResetTokenAsync(user);

    public Task<IdentityResult> ResetPasswordAsync(UserEntity user, string token, string novaSenha) =>
        _userManager.ResetPasswordAsync(user, token, novaSenha);

    public async Task<IdentityResult> UpdatePasswordAsync(UserEntity user, string novaSenha)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return await _userManager.ResetPasswordAsync(user, token, novaSenha);
    }
}
