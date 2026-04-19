using Microsoft.AspNetCore.Identity;

namespace GoodHamburger.API.Entities;

public class UserEntity : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
