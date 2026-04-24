using GoodHamburger.Shared.DTOs.Customers;
using GoodHamburger.Shared.Models.Customers;

namespace GoodHamburger.Portal.Services.Customers;

public class CustomerService
{
    private readonly HttpClient _http;

    public CustomerService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Obtém todos os clientes cadastrados.
    /// </summary>
    /// <exception cref="HttpRequestException">401 Unauthorized, 403 Forbidden.</exception>
    public Task<List<Customer>> GetAllAsync() =>
        _http.GetFromJsonAsync<List<Customer>>("api/customers")!;

    /// <summary>
    /// Obtém um cliente pelo ID.
    /// </summary>
    /// <param name="id">ID do cliente.</param>
    /// <exception cref="HttpRequestException">401 Unauthorized, 403 Forbidden, 404 Not Found.</exception>
    public Task<Customer> GetByIdAsync(Guid id) =>
        _http.GetFromJsonAsync<Customer>($"api/customers/{id}")!;

    /// <summary>
    /// Cadastra um novo cliente.
    /// </summary>
    /// <param name="dto">Dados do cliente.</param>
    /// <exception cref="HttpRequestException">400 Bad Request, 401 Unauthorized, 403 Forbidden.</exception>
    public async Task<Customer> CreateAsync(CreateCustomerDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/customers", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Customer>())!;
    }

    /// <summary>
    /// Atualiza os dados de um cliente.
    /// </summary>
    /// <param name="dto">Dados atualizados do cliente.</param>
    /// <exception cref="HttpRequestException">400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found.</exception>
    public async Task<Customer> UpdateAsync(UpdateCustomerDto dto)
    {
        var response = await _http.PutAsJsonAsync("api/customers", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Customer>())!;
    }

    /// <summary>
    /// Remove um cliente pelo ID.
    /// </summary>
    /// <param name="id">ID do cliente.</param>
    /// <exception cref="HttpRequestException">400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found.</exception>
    public Task DeleteAsync(Guid id) =>
        _http.DeleteAsync($"api/customers/{id}");

    /// <summary>
    /// Busca clientes por nome e/ou telefone.
    /// </summary>
    /// <param name="name">Nome ou parte do nome (opcional).</param>
    /// <param name="phone">Telefone ou parte (opcional).</param>
    /// <exception cref="HttpRequestException">401 Unauthorized, 403 Forbidden.</exception>
    public async Task<List<Customer>> FindAsync(string? name = null, string? phone = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(name)) query.Add($"Name={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrWhiteSpace(phone)) query.Add($"Phone={Uri.EscapeDataString(phone)}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
        return (await _http.GetFromJsonAsync<List<Customer>>($"api/customers/search{qs}"))!;
    }
}