using GoodHamburger.API.Repositories.Auth;
using GoodHamburger.API.Services.Auth;
using GoodHamburger.Shared.Common;
using GoodHamburger.Shared.Constants;
using GoodHamburger.Shared.DTOs.Auth;
using GoodHamburger.Shared.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoodHamburger.API.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;

    public AuthController(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUserService userService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _userService = userService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("User login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);

        if (user is null || !user.IsActive)
            return Unauthorized(new ErrorResponse("Credenciais inválidas ou usuário inativo."));

        var isValidPassword = await _userRepository.VerifyPasswordAsync(user, dto.Senha);
        if (!isValidPassword)
            return Unauthorized(new ErrorResponse("Credenciais inválidas ou usuário inativo."));

        var token = await _tokenService.GerarTokenAsync(user);
        return Ok(token);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Refresh access token")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto, CancellationToken cancellationToken)
    {
        var tokenResponse = await _tokenService.RefreshTokenAsync(dto.RefreshToken, cancellationToken);
        if (tokenResponse == null)
            return BadRequest(new ErrorResponse("Refresh token inválido ou expirado."));

        return Ok(tokenResponse);
    }

    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Revoke all user tokens")]
    public async Task<IActionResult> Revoke(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ErrorResponse("Usuário não identificado."));

        await _tokenService.RevokeAllUserTokensAsync(Guid.Parse(userId), cancellationToken);
        return NoContent();
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [EndpointSummary("Reset user password")]
    public async Task<IActionResult> RedefinirSenha([FromBody] ResetUserPasswordDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.ResetPasswordAsync(dto, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }
}
