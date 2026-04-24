using GoodHamburger.Shared.DTOs.Auth;
using System.Text.Json.Serialization;

namespace GoodHamburger.Portal.Services.Auth;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public AuthService(IHttpClientFactory clientFactory, ILocalStorageService localStorage)
    {
        _httpClient = clientFactory.CreateClient("ApiClient");
        _localStorage = localStorage;
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            var loginDto = new LoginDto(email, password);
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
                if (tokenResponse is not null)
                {
                    await _localStorage.SetItemAsync("access_token", tokenResponse.Token);
                    await _localStorage.SetItemAsync("refresh_token", tokenResponse.RefreshToken);
                    return new LoginResult { Succeeded = true };
                }
            }

            return new LoginResult
            {
                Succeeded = false,
                Error = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "Credenciais inválidas ou usuário inativo."
                    : "Erro ao fazer login."
            };
        }
        catch (Exception ex)
        {
            return new LoginResult { Succeeded = false, Error = ex.Message };
        }
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _localStorage.GetItemAsync<string>("refresh_token");
            if (string.IsNullOrEmpty(refreshToken))
                return null;

            var refreshDto = new RefreshTokenDto(refreshToken);
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", refreshDto);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
                if (tokenResponse is not null)
                {
                    await _localStorage.SetItemAsync("access_token", tokenResponse.Token);
                    await _localStorage.SetItemAsync("refresh_token", tokenResponse.RefreshToken);
                    return tokenResponse;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _httpClient.PostAsync("api/auth/revoke", null);
        }
        catch { }
        finally
        {
            await _localStorage.RemoveItemAsync("access_token");
            await _localStorage.RemoveItemAsync("refresh_token");
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("access_token");
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("refresh_token");
    }
}

public class LoginResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
}
