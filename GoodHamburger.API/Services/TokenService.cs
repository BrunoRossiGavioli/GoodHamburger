using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GoodHamburger.API.Entities;
using GoodHamburger.API.Repositories;
using GoodHamburger.Shared.DTOs.Auth;
using Microsoft.IdentityModel.Tokens;

namespace GoodHamburger.API.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IUsuarioRepository _usuarioRepository;

    public TokenService(IConfiguration configuration, IUsuarioRepository usuarioRepository)
    {
        _configuration = configuration;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<TokenResponseDto> GerarTokenAsync(UserEntity usuario)
    {
        var roles = await _usuarioRepository.ObterRolesAsync(usuario);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Email, usuario.Email!),
            new(ClaimTypes.Name, usuario.Name)
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
            usuario.Email!,
            usuario.Name,
            roles,
            expiracao);
    }
}
