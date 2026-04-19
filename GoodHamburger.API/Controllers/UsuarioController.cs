using GoodHamburger.API.Services;
using GoodHamburger.Shared.Common;
using GoodHamburger.Shared.Constants;
using GoodHamburger.Shared.DTOs.Usuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = Roles.Administrador)]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioService.ObterTodosAsync(cancellationToken);
        return Ok(usuarios);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _usuarioService.ObterPorIdAsync(id, cancellationToken);
            return Ok(usuario);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarUsuarioDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _usuarioService.CriarAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Alterar(Guid id, [FromBody] AlterarUsuarioDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _usuarioService.AlterarAsync(id, dto, cancellationToken);
            return Ok(usuario);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id:guid}/senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarSenha(Guid id, [FromBody] AlterarSenhaAdminDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _usuarioService.AlterarSenhaAdminAsync(id, dto, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id:guid}/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _usuarioService.InativarAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }
}
