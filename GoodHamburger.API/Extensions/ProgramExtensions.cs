using GoodHamburger.API.Data;
using GoodHamburger.API.Data.Seeder;
using GoodHamburger.API.Entities.Auth;
using GoodHamburger.API.Repositories.Auth;
using GoodHamburger.API.Repositories.Customers;
using GoodHamburger.API.Repositories.Products;
using GoodHamburger.API.Repositories.PurchaseOrders;
using GoodHamburger.API.Services.Auth;
using GoodHamburger.API.Services.Customers;
using GoodHamburger.API.Services.Emails;
using GoodHamburger.API.Services.Products;
using GoodHamburger.API.Services.PurchaseOrdens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace GoodHamburger.API.Extensions;

public static class ProgramExtensions
{
    public static IServiceCollection AddOpenApi(this WebApplicationBuilder builder)
    {
        return builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes.TryAdd("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Informe o token JWT obtido no endpoint /api/auth/login."
                });

                document.Security =
                [
                    new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer"), [] }
            }
                ];

                document.SetReferenceHostDocument();
                return Task.CompletedTask;
            });
        });
    }

    public static IServiceCollection AddDbContext(this WebApplicationBuilder builder)
    {
        return builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
    }

    public static IServiceCollection AddIdentityAndAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<UserEntity, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        }).AddEntityFrameworkStores<AppDbContext>()
          .AddDefaultTokenProviders();

        var jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key não configurada.");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

        return builder.Services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductPriceRepository, ProductPriceRepository>();

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<ICustomerService, CustomerService>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductPriceService, ProductPriceService>();

        services.AddScoped<IOrderService, OrderService>();

        return services;
    }

    public static async Task<WebApplication> ApplyDbMigrationsAndSeed(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(scope.ServiceProvider);

        return app;
    }
}
