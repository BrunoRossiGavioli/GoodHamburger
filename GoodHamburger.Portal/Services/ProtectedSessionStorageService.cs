using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;

namespace GoodHamburger.Portal.Services;

public class ProtectedSessionStorageService : ILocalStorageService
{
    private readonly ProtectedSessionStorage _sessionStorage;

    public ProtectedSessionStorageService(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            await _sessionStorage.SetAsync(key, value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao armazenar item: {ex.Message}");
        }
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var result = await _sessionStorage.GetAsync<T>(key);
            return result.Success ? result.Value : default;
        }
        catch
        {
            return default;
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _sessionStorage.DeleteAsync(key);
        }
        catch { }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _sessionStorage.DeleteAsync("access_token");
            await _sessionStorage.DeleteAsync("refresh_token");
        }
        catch { }
    }
}
