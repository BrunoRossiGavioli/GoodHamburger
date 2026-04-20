using GoodHamburger.API.Repositories.Auth;
using GoodHamburger.API.Services.Auth;
using GoodHamburger.Shared.Common;
using GoodHamburger.Shared.DTOs.Auth;
using GoodHamburger.Shared.DTOs.Usuario;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.API.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _UserRepository;
    private readonly ITokenService _tokenService;
    private readonly IUserService _usuarioService;

    public AuthController(
        IUserRepository UserRepository,
        ITokenService tokenService,
        IUserService usuarioService)
    {
        _UserRepository = UserRepository;
        _tokenService = tokenService;
        _usuarioService = usuarioService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var usuario = await _UserRepository.ObterPorEmailAsync(dto.Email, cancellationToken);

        if (usuario is null || !usuario.IsActive)
            return Unauthorized(new ErrorResponse("Credenciais inválidas ou usuário inativo."));

        var senhaValida = await _UserRepository.VerificarSenhaAsync(usuario, dto.Senha);
        if (!senhaValida)
            return Unauthorized(new ErrorResponse("Credenciais inválidas ou usuário inativo."));

        var token = await _tokenService.GerarTokenAsync(usuario);
        return Ok(token);
    }

    [HttpPost("redefinir-senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _usuarioService.RedefinirSenhaAsync(dto, cancellationToken);
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
