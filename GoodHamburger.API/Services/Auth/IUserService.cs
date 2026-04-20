using GoodHamburger.Shared.DTOs.Products;
using GoodHamburger.Shared.DTOs.Users;
using GoodHamburger.Shared.Models.Users;

namespace GoodHamburger.API.Services.Auth;

public interface IUserService
{
    Task<User> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAll(CancellationToken cancellationToken = default);
    Task<User> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<User> UpdateAsync(UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task UpdatePasswordAsync(ChangeUserPasswordDto dto, CancellationToken cancellationToken = default);
    Task UpdateActiveState(UpdateUserActiveStateDto dto, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetUserPasswordDto dto, CancellationToken cancellationToken = default);
}
