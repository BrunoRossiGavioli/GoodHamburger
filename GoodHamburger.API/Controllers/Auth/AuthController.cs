using GoodHamburger.API.Repositories.Auth;
using GoodHamburger.API.Services.Auth;
using GoodHamburger.Shared.Common;
using GoodHamburger.Shared.DTOs.Auth;
using GoodHamburger.Shared.DTOs.Users;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.API.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _UserRepository;
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;

    public AuthController(
        IUserRepository UserRepository,
        ITokenService tokenService,
        IUserService userService)
    {
        _UserRepository = UserRepository;
        _tokenService = tokenService;
        _userService = userService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var user = await _UserRepository.GetByEmailAsync(dto.Email, cancellationToken);

        if (user is null || !user.IsActive)
            return Unauthorized(new ErrorResponse("Credenciais inválidas ou usuário inativo."));

        var isValidPassword = await _UserRepository.VerifyPasswordAsync(user, dto.Senha);
        if (!isValidPassword)
            return Unauthorized(new ErrorResponse("Credenciais inválidas ou usuário inativo."));

        var token = await _tokenService.GerarTokenAsync(user);
        return Ok(token);
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
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
