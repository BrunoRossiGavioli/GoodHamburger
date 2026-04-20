using GoodHamburger.API.Entities.Auth;
using GoodHamburger.Shared.DTOs.Auth;

namespace GoodHamburger.API.Services.Auth;

public interface ITokenService
{
    Task<TokenResponseDto> GerarTokenAsync(UserEntity usuario);
}
