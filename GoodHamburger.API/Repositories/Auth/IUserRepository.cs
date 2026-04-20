using GoodHamburger.API.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace GoodHamburger.API.Repositories.Auth;

public interface IUserRepository
{
    Task<UserEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserEntity>> GetAll(CancellationToken cancellationToken = default);
    Task<IdentityResult> CreateAsync(UserEntity user, string password);
    Task<IdentityResult> UpdateAsync(UserEntity user);
    Task<IdentityResult> AddRole(UserEntity user, string role);
    Task<IdentityResult> RemoveRole(UserEntity user, string role);
    Task<IList<string>> GetAllRoles(UserEntity user);
    Task<bool> VerifyPasswordAsync(UserEntity user, string password);
    Task<string> GenerateTokenPasswordResetAsync(UserEntity user);
    Task<IdentityResult> ResetPasswordAsync(UserEntity user, string token, string newPassword);
    Task<IdentityResult> UpdatePasswordAsync(UserEntity user, string newPassword);
}
