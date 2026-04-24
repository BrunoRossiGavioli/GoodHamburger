using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GoodHamburger.Portal.Services.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider, IAsyncDisposable
{
    private readonly AuthService _authService;
    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _js;

    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

    // Referência .NET exposta ao JavaScript para callbacks do BroadcastChannel.
    // DotNetObjectReference.Create(this) funciona em qualquer classe C#, não apenas componentes.
    private DotNetObjectReference<CustomAuthStateProvider>? _dotNetRef;

    // Módulo ES carregado lazy para evitar uso antes do circuito estar ativo.
    private IJSObjectReference? _authSyncModule;

    public CustomAuthStateProvider(
        AuthService authService,
        ILocalStorageService localStorage,
        IJSRuntime js)
    {
        _authService = authService;
        _localStorage = localStorage;
        _js = js;
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
        // Broadcast ANTES de limpar tokens: outras abas precisam receber a mensagem
        // enquanto o canal desta aba ainda está ativo.
        await BroadcastLogoutAsync();

        await _authService.LogoutAsync();
        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    /// Inicializa o listener do BroadcastChannel para esta aba.
    /// Deve ser chamado em OnAfterRenderAsync(firstRender: true) do MainLayout,
    /// após o circuito Blazor estar ativo (JS interop disponível).
    /// </summary>
    public async Task InitializeBroadcastAsync()
    {
        try
        {
            // Lazy-load do módulo ES: carrega uma única vez por circuito
            _authSyncModule ??= await _js.InvokeAsync<IJSObjectReference>(
                "import", "./js/authSync.js");

            _dotNetRef ??= DotNetObjectReference.Create(this);

            await _authSyncModule.InvokeVoidAsync("initAuthSync", _dotNetRef);
        }
        catch (JSException)
        {
            // Ambiente de teste ou SSR residual — ignora silenciosamente.
        }
    }

    /// <summary>
    /// Callback invocado pelo JavaScript (BroadcastChannel) quando outra aba faz logout.
    /// Executa o logout local sem chamar a API: a aba origem já revogou o token server-side.
    /// </summary>
    [JSInvokable]
    public async Task OnRemoteLogout()
    {
        await _localStorage.ClearAsync();
        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private async Task BroadcastLogoutAsync()
    {
        try
        {
            if (_authSyncModule is not null)
                await _authSyncModule.InvokeVoidAsync("broadcastLogout");
        }
        catch (JSException)
        {
            // Falha de JS não impede o logout local.
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_authSyncModule is not null)
        {
            try
            {
                await _authSyncModule.InvokeVoidAsync("disposeAuthSync");
                await _authSyncModule.DisposeAsync();
            }
            catch { }
        }
        _dotNetRef?.Dispose();
    }

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo < Datetime.Now.AddMinutes(5);
        }
        catch
        {
            return true;
        }
    }

    private static List<Claim> ParseTokenClaims(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.ToList();
        }
        catch
        {
            return [];
        }
    }
}
