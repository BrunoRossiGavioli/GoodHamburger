using GoodHamburger.Shared.DTOs.Users;

namespace GoodHamburger.API.Services.Auth;

public interface IUserService
{
    Task<GetUserDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GetUserDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<GetUserDto> CriarAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<GetUserDto> AlterarAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task AlterarSenhaAdminAsync(Guid id, ChangePasswordAdminDto dto, CancellationToken cancellationToken = default);
    Task InativarAsync(Guid id, CancellationToken cancellationToken = default);
    Task RedefinirSenhaAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
}
