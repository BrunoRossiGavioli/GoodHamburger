using GoodHamburger.API.Entities;
using GoodHamburger.API.Repositories;
using GoodHamburger.Shared.Constants;
using GoodHamburger.Shared.DTOs.Usuario;
using Microsoft.AspNetCore.Identity;

namespace GoodHamburger.API.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailService _emailService;

    public UsuarioService(IUsuarioRepository usuarioRepository, IEmailService emailService)
    {
        _usuarioRepository = usuarioRepository;
        _emailService = emailService;
    }

    public async Task<UsuarioResponseDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");

        var roles = await _usuarioRepository.ObterRolesAsync(usuario);
        return MapToDto(usuario, roles);
    }

    public async Task<IReadOnlyList<UsuarioResponseDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await _usuarioRepository.ObterTodosAsync(cancellationToken);

        var result = new List<UsuarioResponseDto>(usuarios.Count);
        foreach (var usuario in usuarios)
        {
            var roles = await _usuarioRepository.ObterRolesAsync(usuario);
            result.Add(MapToDto(usuario, roles));
        }

        return result;
    }

    public async Task<UsuarioResponseDto> CriarAsync(CriarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        ValidarRole(dto.Role);

        var usuario = new UsuarioEntity
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Email = dto.Email,
            UserName = dto.Email,
            EmailConfirmed = true,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var createResult = await _usuarioRepository.CriarAsync(usuario, dto.Senha);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(FormatErrors(createResult));

        var roleResult = await _usuarioRepository.AdicionarRoleAsync(usuario, dto.Role);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException(FormatErrors(roleResult));

        return MapToDto(usuario, [dto.Role]);
    }

    public async Task<UsuarioResponseDto> AlterarAsync(Guid id, AlterarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");

        if (!string.IsNullOrWhiteSpace(dto.Nome))
            usuario.Nome = dto.Nome;

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != usuario.Email)
        {
            usuario.Email = dto.Email;
            usuario.UserName = dto.Email;
        }

        var updateResult = await _usuarioRepository.AtualizarAsync(usuario);
        if (!updateResult.Succeeded)
            throw new InvalidOperationException(FormatErrors(updateResult));

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            ValidarRole(dto.Role);
            var rolesAtuais = await _usuarioRepository.ObterRolesAsync(usuario);
            foreach (var role in rolesAtuais)
                await _usuarioRepository.RemoverRoleAsync(usuario, role);

            await _usuarioRepository.AdicionarRoleAsync(usuario, dto.Role);
        }

        var roles = await _usuarioRepository.ObterRolesAsync(usuario);
        return MapToDto(usuario, roles);
    }

    public async Task AlterarSenhaAdminAsync(Guid id, AlterarSenhaAdminDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");

        var result = await _usuarioRepository.AlterarSenhaAdminAsync(usuario, dto.NovaSenha);
        if (!result.Succeeded)
            throw new InvalidOperationException(FormatErrors(result));
    }

    public async Task InativarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");

        if (!usuario.Ativo)
            throw new InvalidOperationException("Usuário já está inativo.");

        usuario.Ativo = false;
        var updateResult = await _usuarioRepository.AtualizarAsync(usuario);
        if (!updateResult.Succeeded)
            throw new InvalidOperationException(FormatErrors(updateResult));

        var token = await _usuarioRepository.GerarTokenRedefinicaoSenhaAsync(usuario);
        await _emailService.EnviarRedefinicaoSenhaAsync(usuario.Email!, usuario.Nome, token, cancellationToken);
    }

    public async Task RedefinirSenhaAsync(RedefinirSenhaDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(dto.Email, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        var result = await _usuarioRepository.RedefinirSenhaAsync(usuario, dto.Token, dto.NovaSenha);
        if (!result.Succeeded)
            throw new InvalidOperationException(FormatErrors(result));
    }

    private static UsuarioResponseDto MapToDto(UsuarioEntity usuario, IEnumerable<string> roles) =>
        new(usuario.Id, usuario.Nome, usuario.Email!, usuario.Ativo, usuario.CriadoEm, roles);

    private static void ValidarRole(string role)
    {
        string[] validas = [Roles.Administrador, Roles.Funcionario];
        if (!validas.Contains(role))
            throw new ArgumentException($"Role inválida: '{role}'. Válidas: {string.Join(", ", validas)}");
    }

    private static string FormatErrors(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(e => e.Description));
}
