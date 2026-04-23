using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GoodHamburger.Portal.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _authService;
    private readonly ILocalStorageService _localStorage;
    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

    public CustomAuthStateProvider(AuthService authService, ILocalStorageService localStorage)
    {
        _authService = authService;
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _authService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            if (IsTokenExpired(token))
            {
                var refreshed = await _authService.RefreshTokenAsync();
                if (refreshed is null)
                {
                    await _authService.LogoutAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
                token = refreshed.Token;
            }

            var claims = ParseTokenClaims(token);
            _cachedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

            return new AuthenticationState(_cachedUser);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task NotifyLoginAsync(string token)
    {
        var claims = ParseTokenClaims(token);
        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        await Task.CompletedTask;
    }

    public async Task NotifyLogoutAsync()
    {
        await _authService.LogoutAsync();
        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow.AddMinutes(5);
        }
        catch
        {
            return true;
        }
    }

    private static List<Claim> ParseTokenClaims(string token)
    {
        var claims = new List<Claim>();
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            claims = jwtToken.Claims.ToList();
        }
        catch { }
        return claims;
    }
}
