using GoodHamburger.API.Entities.Auth;
using GoodHamburger.API.Repositories.Auth;
using GoodHamburger.Shared.DTOs.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GoodHamburger.API.Services.Auth;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _UserRepository;

    public TokenService(IConfiguration configuration, IUserRepository UserRepository)
    {
        _configuration = configuration;
        _UserRepository = UserRepository;
    }

    public async Task<TokenResponseDto> GerarTokenAsync(UserEntity user)
    {
        var roles = await _UserRepository.GetAllRoles(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.Name)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60");
        var expiracao = DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiracao,
            signingCredentials: credentials);

        return new TokenResponseDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Email!,
            user.Name,
            roles,
            expiracao);
    }
}
