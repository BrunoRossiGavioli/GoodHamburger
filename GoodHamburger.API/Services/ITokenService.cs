using GoodHamburger.API.Entities;
using GoodHamburger.Shared.DTOs.Auth;

namespace GoodHamburger.API.Services;

public interface ITokenService
{
    Task<TokenResponseDto> GerarTokenAsync(UserEntity usuario);
}
