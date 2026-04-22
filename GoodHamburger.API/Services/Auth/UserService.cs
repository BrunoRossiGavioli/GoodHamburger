using GoodHamburger.API.Entities.Auth;
using GoodHamburger.API.Repositories.Auth;
using GoodHamburger.API.Services.Emails;
using GoodHamburger.Shared.Constants;
using GoodHamburger.Shared.DTOs.Users;
using GoodHamburger.Shared.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace GoodHamburger.API.Services.Auth;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public UserService(IUserRepository UserRepository, IEmailService emailService)
    {
        _userRepository = UserRepository;
        _emailService = emailService;
    }

    public async Task<User> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");

        var roles = await _userRepository.GetAllRoles(user);
        return MapToDto(user, roles);
    }

    public async Task<IReadOnlyList<User>> GetAll(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAll(cancellationToken);

        var result = new List<User>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userRepository.GetAllRoles(user);
            result.Add(MapToDto(user, roles));
        }

        return result;
    }

    public async Task<User> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        ValidarRole(dto.Role);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userRepository.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(FormatErrors(createResult));

        var roleResult = await _userRepository.AddRole(user, dto.Role);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException(FormatErrors(roleResult));

        return MapToDto(user, [dto.Role]);
    }

    public async Task<User> UpdateAsync(UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(dto.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {dto.Id} não encontrado.");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            user.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            user.Email = dto.Email;
            user.UserName = dto.Email;
        }

        var updateResult = await _userRepository.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new InvalidOperationException(FormatErrors(updateResult));

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            ValidarRole(dto.Role);
            var rolesAtuais = await _userRepository.GetAllRoles(user);
            foreach (var role in rolesAtuais)
                await _userRepository.RemoveRole(user, role);

            await _userRepository.AddRole(user, dto.Role);
        }

        var roles = await _userRepository.GetAllRoles(user);
        return MapToDto(user, roles);
    }

    public async Task UpdatePasswordAsync(ChangeUserPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(dto.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {dto.Id} não encontrado.");

        var result = await _userRepository.UpdatePasswordAsync(user, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(FormatErrors(result));
    }

    public async Task UpdateActiveState(UpdateUserActiveStateDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(dto.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {dto.Id} não encontrado.");

        if (!user.IsActive && !dto.IsActive || user.IsActive && dto.IsActive)
            return;

        user.IsActive = dto.IsActive;
        var updateResult = await _userRepository.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new InvalidOperationException(FormatErrors(updateResult));

        var token = await _userRepository.GenerateTokenPasswordResetAsync(user);
        await _emailService.EnviarRedefinicaoSenhaAsync(user.Email!, user.Name, token, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetUserPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        var result = await _userRepository.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(FormatErrors(result));
    }

    private static User MapToDto(UserEntity user, IEnumerable<string> roles) =>
        new(user.Id, user.Name, user.Email!, user.IsActive, user.CreatedAt, roles);

    private static void ValidarRole(string role)
    {
        string[] validas = [Roles.Administrador, Roles.Funcionario];
        if (!validas.Contains(role))
            throw new ArgumentException($"Role inválida: '{role}'. Válidas: {string.Join(", ", validas)}");
    }

    private static string FormatErrors(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(e => e.Description));
}
