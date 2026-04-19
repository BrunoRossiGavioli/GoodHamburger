using GoodHamburger.API.Entities;
using GoodHamburger.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace GoodHamburger.API.Data.Seeder;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<UsuarioEntity>>();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles = [Roles.Administrador, Roles.Funcionario];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    private static async Task SeedAdminAsync(UserManager<UsuarioEntity> userManager)
    {
        const string adminEmail = "admin@admin.com";
        const string adminSenha = "Admin123!";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new UsuarioEntity
        {
            Id = Guid.NewGuid(),
            Nome = "Administrador",
            Email = adminEmail,
            UserName = adminEmail,
            EmailConfirmed = true,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, adminSenha);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, Roles.Administrador);
    }
}
