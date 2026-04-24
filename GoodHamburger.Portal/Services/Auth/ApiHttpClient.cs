// Services/ApiHttpClient.cs
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using GoodHamburger.Shared.DTOs.Auth;

namespace GoodHamburger.Portal.Services.Auth;

public class ApiHttpClient
{
    private readonly HttpClient _http;
    private readonly ProtectedLocalStorage _localStorage;

    public ApiHttpClient(HttpClient http, ProtectedLocalStorage sessionStorage)
    {
        _http = http;
        _localStorage = sessionStorage;
    }

    /// <summary>
    /// Adiciona o token ao header e executa a requisição.
    /// Em caso de 401, tenta renovar o token e repete a requisição uma vez.
    /// </summary>
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync(request, cancellationToken);

        var response = await _http.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            if (await TryRefreshTokenAsync(cancellationToken))
            {
                // Recria a requisição (não reusamos a mesma) com o novo token
                var newRequest = await CloneRequestAsync(request, cancellationToken);
                await AttachTokenAsync(newRequest, cancellationToken);
                response = await _http.SendAsync(newRequest, cancellationToken);
            }
        }

        return response;
    }

    // Métodos auxiliares para GET, POST, PUT, DELETE
    public async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T value, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(value)
        };
        return await SendAsync(request, cancellationToken);
    }

    public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T value, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(value)
        };
        return await SendAsync(request, cancellationToken);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        return await SendAsync(request, cancellationToken);
    }

    private async Task AttachTokenAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var tokenResult = await _localStorage.GetAsync<string>("access_token");
        if (tokenResult.Success && !string.IsNullOrEmpty(tokenResult.Value))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value);
        }
    }

    private async Task<bool> TryRefreshTokenAsync(CancellationToken ct)
    {
        var refreshResult = await _localStorage.GetAsync<string>("refresh_token");
        if (!refreshResult.Success || string.IsNullOrEmpty(refreshResult.Value))
            return false;

        // Usamos o mesmo HttpClient, mas sem token anexado
        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh")
        {
            Content = JsonContent.Create(new { refreshToken = refreshResult.Value })
        };

        var response = await _http.SendAsync(request, ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<TokenResponseDto>(cancellationToken: ct);
            await _localStorage.SetAsync("access_token", result!.Token);
            await _localStorage.SetAsync("refresh_token", result.RefreshToken);
            return true;
        }

        return false;
    }

    private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var content = await request.Content.ReadAsByteArrayAsync(ct);
            clone.Content = new ByteArrayContent(content);
            if (request.Content.Headers.ContentType != null)
                clone.Content.Headers.ContentType = request.Content.Headers.ContentType;
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}