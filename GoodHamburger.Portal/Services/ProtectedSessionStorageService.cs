using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace GoodHamburger.Portal.Services;

// Usa ProtectedLocalStorage (compartilhado entre abas do mesmo domínio)
// em vez de ProtectedSessionStorage (isolado por aba/circuito).
// Os dados são criptografados pela Data Protection API do ASP.NET Core.
public class ProtectedSessionStorageService : ILocalStorageService
{
    private readonly ProtectedLocalStorage _localStorage;

    public ProtectedSessionStorageService(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            await _localStorage.SetAsync(key, value);
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
            var result = await _localStorage.GetAsync<T>(key);
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
            await _localStorage.DeleteAsync(key);
        }
        catch { }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _localStorage.DeleteAsync("access_token");
            await _localStorage.DeleteAsync("refresh_token");
        }
        catch { }
    }
}
