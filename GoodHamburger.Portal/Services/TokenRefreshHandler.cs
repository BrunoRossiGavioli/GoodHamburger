using GoodHamburger.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net;
using System.Net.Http.Headers;

public class TokenRefreshHandler : DelegatingHandler
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly IHttpClientFactory _clientFactory;

    public TokenRefreshHandler(
        ProtectedSessionStorage sessionStorage,
        IHttpClientFactory clientFactory)
    {
        _sessionStorage = sessionStorage;
        _clientFactory = clientFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Anexa o access token (tolerante a prerender/SSR onde JS interop não está disponível)
        var token = await TryGetItemAsync("access_token");
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshed = await TryRefreshTokenAsync(cancellationToken);
            if (refreshed)
            {
                // Atualiza o token na requisição repetida
                token = await TryGetItemAsync("access_token");
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                response = await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }

    private async Task<bool> TryRefreshTokenAsync(CancellationToken ct)
    {
        var refreshToken = await TryGetItemAsync("refresh_token");
        if (string.IsNullOrEmpty(refreshToken))
            return false;

        var client = _clientFactory.CreateClient();
        var refreshResponse = await client.PostAsJsonAsync("api/auth/refresh",
            new { refreshToken }, ct);

        if (refreshResponse.IsSuccessStatusCode)
        {
            var tokenResponse = await refreshResponse.Content.ReadFromJsonAsync<TokenResponseDto>(cancellationToken: ct);
            if(tokenResponse is null)
                return false;

            await TrySetItemAsync("access_token", tokenResponse.Token);
            await TrySetItemAsync("refresh_token", tokenResponse.RefreshToken);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Leitura do ProtectedSessionStorage resiliente: durante prerender/SSR o JS interop
    /// não está disponível e lança InvalidOperationException. Nesse caso, retornamos null
    /// e a requisição prossegue sem Authorization header (será refeita quando o circuito subir).
    /// </summary>
    private async Task<string?> TryGetItemAsync(string key)
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(key);
            return result.Success ? result.Value : null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private async Task TrySetItemAsync(string key, string value)
    {
        try
        {
            await _sessionStorage.SetAsync(key, value);
        }
        catch (InvalidOperationException)
        {
            // JS interop indisponível durante prerender — ignorar silenciosamente.
        }
    }
}