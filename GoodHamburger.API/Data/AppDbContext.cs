using GoodHamburger.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.API.Data;

public class AppDbContext : IdentityDbContext<UsuarioEntity, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UsuarioEntity>(entity =>
        {
            entity.Property(u => u.Nome).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Ativo).HasDefaultValue(true);
        });
    }
}
