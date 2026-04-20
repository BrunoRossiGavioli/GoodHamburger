using GoodHamburger.API.Entities.Auth;
using GoodHamburger.API.Repositories.Auth;
using GoodHamburger.API.Services.Emails;
using GoodHamburger.Shared.Constants;
using GoodHamburger.Shared.DTOs.Users;
using Microsoft.AspNetCore.Identity;

namespace GoodHamburger.API.Services.Auth;

public class UserService : IUserService
{
    private readonly IUserRepository _UserRepository;
    private readonly IEmailService _emailService;

    public UserService(IUserRepository UserRepository, IEmailService emailService)
    {
        _UserRepository = UserRepository;
        _emailService = emailService;
    }

    public async Task<GetUserDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var usuario = await _UserRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");

        var roles = await _UserRepository.ObterRolesAsync(usuario);
        return MapToDto(usuario, roles);
    }

    public async Task<IReadOnlyList<GetUserDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await _UserRepository.ObterTodosAsync(cancellationToken);

        var result = new List<GetUserDto>(usuarios.Count);
        foreach (var usuario in usuarios)
        {
            var roles = await _UserRepository.ObterRolesAsync(usuario);
            result.Add(MapToDto(usuario, roles));
        }

        return result;
    }

    public async Task<GetUserDto> CriarAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        ValidarRole(dto.Role);

        var usuario = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _UserRepository.CriarAsync(usuario, dto.Password);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(FormatErrors(createResult));

        var roleResult = await _UserRepository.AdicionarRoleAsync(usuario, dto.Role);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException(FormatErrors(roleResult));

        return MapToDto(usuario, [dto.Role]);
    }

    public async Task<GetUserDto> AlterarAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _UserRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            usuario.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != usuario.Email)
        {
            usuario.Email = dto.Email;
            usuario.UserName = dto.Email;
        }

        var updateResult = await _UserRepository.AtualizarAsync(usuario);
        if (!updateResult.Succeeded)
            throw new InvalidOperationException(FormatErrors(updateResult));

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            ValidarRole(dto.Role);
            var rolesAtuais = await _UserRepository.ObterRolesAsync(usuario);
            foreach (var role in rolesAtuais)
                await _UserRepository.RemoverRoleAsync(usuario, role);

            await _UserRepository.AdicionarRoleAsync(usuario, dto.Role);
        }

        var roles = await _UserRepository.ObterRolesAsync(usuario);
        return MapToDto(usuario, roles);
    }

    public async Task AlterarSenhaAdminAsync(Guid id, ChangePasswordAdminDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _UserRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");

        var result = await _UserRepository.AlterarSenhaAdminAsync(usuario, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(FormatErrors(result));
    }

    public async Task InativarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var usuario = await _UserRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");

        if (!usuario.IsActive)
            throw new InvalidOperationException("Usuário já está inativo.");

        usuario.IsActive = false;
        var updateResult = await _UserRepository.AtualizarAsync(usuario);
        if (!updateResult.Succeeded)
            throw new InvalidOperationException(FormatErrors(updateResult));

        var token = await _UserRepository.GerarTokenRedefinicaoSenhaAsync(usuario);
        await _emailService.EnviarRedefinicaoSenhaAsync(usuario.Email!, usuario.Name, token, cancellationToken);
    }

    public async Task RedefinirSenhaAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _UserRepository.ObterPorEmailAsync(dto.Email, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        var result = await _UserRepository.RedefinirSenhaAsync(usuario, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(FormatErrors(result));
    }

    private static GetUserDto MapToDto(UserEntity usuario, IEnumerable<string> roles) =>
        new(usuario.Id, usuario.Name, usuario.Email!, usuario.IsActive, usuario.CreatedAt, roles);

    private static void ValidarRole(string role)
    {
        string[] validas = [Roles.Administrador, Roles.Funcionario];
        if (!validas.Contains(role))
            throw new ArgumentException($"Role inválida: '{role}'. Válidas: {string.Join(", ", validas)}");
    }

    private static string FormatErrors(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(e => e.Description));
}
