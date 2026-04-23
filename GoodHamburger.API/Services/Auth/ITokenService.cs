using GoodHamburger.API.Entities.Auth;
using GoodHamburger.Shared.DTOs.Auth;

namespace GoodHamburger.API.Services.Auth;

public interface ITokenService
{
    Task<TokenResponseDto> GerarTokenAsync(UserEntity user);
    Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken);
}
