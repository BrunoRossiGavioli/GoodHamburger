using GoodHamburger.API.Entities.Auth;
using GoodHamburger.API.Entities.Products;
using GoodHamburger.Shared.Constants;
using GoodHamburger.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.API.Data.Seeder;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedProductsAsync(dbContext);
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

    private static async Task SeedUsersAsync(UserManager<UserEntity> userManager)
    {
        await SeedAdminAsync(userManager);
        await SeedFuncionarioAsync(userManager);
    }

    private static async Task SeedAdminAsync(UserManager<UserEntity> userManager)
    {
        const string adminEmail = "admin@admin.com";
        const string adminSenha = "Admin123!";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = "Administrador",
            Email = adminEmail,
            UserName = adminEmail,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, adminSenha);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, Roles.Administrador);
    }

    private static async Task SeedFuncionarioAsync(UserManager<UserEntity> userManager)
    {
        const string funcionarioEmail = "funcionario@funcionario.com";
        const string funcionarioSenha = "Funcionario123!";

        if (await userManager.FindByEmailAsync(funcionarioEmail) is not null)
            return;

        var funcionario = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = "Bruno",
            Email = funcionarioEmail,
            UserName = funcionarioEmail,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(funcionario, funcionarioSenha);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(funcionario, Roles.Funcionario);
    }

    private static async Task SeedProductsAsync(AppDbContext dbContext)
    {
        if (await dbContext.Products.AnyAsync())
            return;

        var produtos = new List<ProductEntity>
    {
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0001-4a2b-8c3d-1e5f6a7b8c9d"),
            Name = "X Burger",
            Description = "Hambúrguer com queijo, alface e tomate",
            Type = ProductType.Sandwich,
            IsActive = true,
            Prices = new List<ProductPriceEntity>
            {
                new()
                {
                    Id = Guid.Parse("b1a2c3d4-0001-4e5f-8a7b-9c0d1e2f3a4b"),
                    Value = 18.90m,
                    StartDate = DateTime.UtcNow,
                    Reason = "Preço inicial"
                }
            }
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0002-4a2b-8c3d-2e5f6a7b8c9d"),
            Name = "X Egg",
            Description = "Hambúrguer com ovo, queijo, alface e tomate",
            Type = ProductType.Sandwich,
            IsActive = true,
            Prices = new List<ProductPriceEntity>
            {
                new()
                {
                    Id = Guid.Parse("b1a2c3d4-0002-4e5f-8a7b-9c0d1e2f3a4b"),
                    Value = 16.90m,
                    StartDate = DateTime.UtcNow,
                    Reason = "Preço inicial"
                }
            }
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0003-4a2b-8c3d-3e5f6a7b8c9d"),
            Name = "X Bacon",
            Description = "Hambúrguer com bacon, queijo, alface e tomate",
            Type = ProductType.Sandwich,
            IsActive = true,
            Prices = new List<ProductPriceEntity>
            {
                new()
                {
                    Id = Guid.Parse("b1a2c3d4-0003-4e5f-8a7b-9c0d1e2f3a4b"),
                    Value = 22.90m,
                    StartDate = DateTime.UtcNow,
                    Reason = "Preço inicial"
                }
            }
        },

        new()
        {
            Id = Guid.Parse("a1b2c3d4-0004-4a2b-8c3d-4e5f6a7b8c9d"),
            Name = "Batata Frita",
            Description = "Porção de batata frita crocante",
            Type = ProductType.Fries,
            IsActive = true,
            Prices = new List<ProductPriceEntity>
            {
                new()
                {
                    Id = Guid.Parse("b1a2c3d4-0004-4e5f-8a7b-9c0d1e2f3a4b"),
                    Value = 7.90m,
                    StartDate = DateTime.UtcNow,
                    Reason = "Preço inicial"
                }
            }
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0006-4a2b-8c3d-6e5f6a7b8c9d"),
            Name = "Batata com Cheddar e Bacon",
            Description = "Porção de batata frita com cheddar cremoso e pedaços de bacon",
            Type = ProductType.Fries,
            IsActive = true,
            Prices = new List<ProductPriceEntity>
            {
                new()
                {
                    Id = Guid.Parse("b1a2c3d4-0006-4e5f-8a7b-9c0d1e2f3a4b"),
                    Value = 14.90m,
                    StartDate = DateTime.UtcNow,
                    Reason = "Preço inicial"
                }
            }
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0007-4a2b-8c3d-7e5f6a7b8c9d"),
            Name = "Batata Rústica",
            Description = "Porção de batata rústica temperada com ervas finas",
            Type = ProductType.Fries,
            IsActive = true,
            Prices = new List<ProductPriceEntity>
            {
                new()
                {
                    Id = Guid.Parse("b1a2c3d4-0007-4e5f-8a7b-9c0d1e2f3a4b"),
                    Value = 12.90m,
                    StartDate = DateTime.UtcNow,
                    Reason = "Preço inicial"
                }
            }
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0008-4a2b-8c3d-8e5f6a7b8c9d"),
            Name = "Batata Supreme",
            Description = "Porção de batata frita com queijo, bacon e molho especial",
            Type = ProductType.Fries,
            IsActive = true,
            Prices = new List<ProductPriceEntity>
            {
                new()
                {
                    Id = Guid.Parse("b1a2c3d4-0008-4e5f-8a7b-9c0d1e2f3a4b"),
                    Value = 18.90m,
                    StartDate = DateTime.UtcNow,
                    Reason = "Preço inicial"
                }
            }
        },

        new()
        {
            Id = Guid.Parse("a1b2c3d4-0005-4a2b-8c3d-5e5f6a7b8c9d"),
            Name = "Refrigerante",
            Description = "Lata 350ml",
            Type = ProductType.Drink,
            IsActive = true,
            Prices = new List<ProductPriceEntity>
            {
                new()
                {
                    Id = Guid.Parse("b1a2c3d4-0005-4e5f-8a7b-9c0d1e2f3a4b"),
                    Value = 6.90m,
                    StartDate = DateTime.UtcNow,
                    Reason = "Preço inicial"
                }
            }
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0009-4a2b-8c3d-9e5f6a7b8c9d"),
            Name = "Coca-Cola",
            Description = "Lata 350ml",
            Type = ProductType.Drink,
            IsActive = true,
            Prices = new List<ProductPriceEntity>
            {
                new()
                {
                    Id = Guid.Parse("b1a2c3d4-0009-4e5f-8a7b-9c0d1e2f3a4b"),
                    Value = 7.50m,
                    StartDate = DateTime.UtcNow,
                    Reason = "Preço inicial"
                }
            }
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0010-4a2b-8c3d-0e5f6a7b8c9d"),
            Name = "Suco Natural de Laranja",
            Description = "Suco natural de laranja 300ml",
            Type = ProductType.Drink,
            IsActive = true,
            Prices = new List<ProductPriceEntity>
            {
                new()
                {
                    Id = Guid.Parse("b1a2c3d4-0010-4e5f-8a7b-9c0d1e2f3a4b"),
                    Value = 9.90m,
                    StartDate = DateTime.UtcNow,
                    Reason = "Preço inicial"
                }
            }
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0011-4a2b-8c3d-1e5f6a7b8c9d"),
            Name = "Milkshake de Chocolate",
            Description = "Milkshake cremoso de chocolate 400ml",
            Type = ProductType.Drink,
            IsActive = true,
            Prices = new List<ProductPriceEntity>
            {
                new()
                {
                    Id = Guid.Parse("b1a2c3d4-0011-4e5f-8a7b-9c0d1e2f3a4b"),
                    Value = 16.90m,
                    StartDate = DateTime.UtcNow,
                    Reason = "Preço inicial"
                }
            }
        },
        new()
        {
            Id = Guid.Parse("a1b2c3d4-0012-4a2b-8c3d-2e5f6a7b8c9d"),
            Name = "Água Mineral",
            Description = "Água mineral sem gás 500ml",
            Type = ProductType.Drink,
            IsActive = true,
            Prices = new List<ProductPriceEntity>
            {
                new()
                {
                    Id = Guid.Parse("b1a2c3d4-0012-4e5f-8a7b-9c0d1e2f3a4b"),
                    Value = 4.50m,
                    StartDate = DateTime.UtcNow,
                    Reason = "Preço inicial"
                }
            }
        }
    };

        await dbContext.Products.AddRangeAsync(produtos);
        await dbContext.SaveChangesAsync();
    }
}
