using Microsoft.AspNetCore.Identity;

namespace GoodHamburger.API.Entities;

public class UsuarioEntity : IdentityUser<Guid>
{
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
