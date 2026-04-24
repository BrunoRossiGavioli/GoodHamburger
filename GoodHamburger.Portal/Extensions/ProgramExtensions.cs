using GoodHamburger.Portal.Services.Auth;
using GoodHamburger.Portal.Services.Customers;
using GoodHamburger.Portal.Services.Orders;
using GoodHamburger.Portal.Services.Products;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace GoodHamburger.Portal.Extensions;

public static class ProgramExtensions
{
    /// <summary>
    /// Adds authentication and authorization services required for the portal to the specified service collection.
    /// </summary>
    /// <remarks>This method registers services for authentication state management, local storage, and
    /// authorization core functionality. It is intended to be called during application startup to configure
    /// authentication and authorization dependencies for the portal.</remarks>
    /// <param name="services">The service collection to which authentication and authorization services will be added. Cannot be null.</param>
    /// <returns>The same instance of <see cref="IServiceCollection"/> that was provided, to support method chaining.</returns>
    public static IServiceCollection AddPortalAuthenticationAndAuthorization(this IServiceCollection services)
    {
        services.AddScoped<ProtectedLocalStorage>();
        services.AddScoped<ILocalStorageService, ProtectedSessionStorageService>();
        services.AddScoped<AuthService>();
        services.AddScoped<CustomAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
        services.AddAuthorizationCore();

        return services;
    }

    /// <summary>
    /// Adds a named HTTP client for API access with token refresh support to the service collection.
    /// </summary>
    /// <remarks>The registered HTTP client is named 'ApiClient' and is configured with a base address from
    /// the 'ApiBaseUrl' configuration setting. A message handler for token refresh is also added to the client
    /// pipeline.</remarks>
    /// <param name="services">The service collection to which the HTTP client and related services are added.</param>
    /// <param name="configuration">The application configuration containing the API base URL. Must include the 'ApiBaseUrl' setting.</param>
    /// <returns>The same service collection instance, enabling method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the 'ApiBaseUrl' configuration value is missing or null.</exception>
    public static IServiceCollection AddApiHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<TokenRefreshHandler>();
        services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri(configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl is not configured."));
        }).AddHttpMessageHandler<TokenRefreshHandler>();
        return services;
    }

    /// <summary>
    /// Adds application-specific API services to the dependency injection container.
    /// </summary>
    /// <remarks>This method registers the API services with scoped
    /// lifetimes. Call this method during application startup to ensure these services are available for dependency
    /// injection.</remarks>
    /// <param name="services">The service collection to which the API services will be added. Cannot be null.</param>
    /// <returns>The same instance of <see cref="IServiceCollection"/> that was provided, to allow for method chaining.</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<ProductService>();
        services.AddScoped<OrderService>();
        services.AddScoped<CustomerService>();

        return services;
    }
}
