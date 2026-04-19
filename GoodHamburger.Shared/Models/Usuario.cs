namespace GoodHamburger.Shared.Models;

public sealed record Usuario(
    Guid Id,
    string Nome,
    string Email,
    bool Ativo,
    DateTime CriadoEm,
    IEnumerable<string> Roles);
