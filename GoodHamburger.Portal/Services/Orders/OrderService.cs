using GoodHamburger.Shared.DTOs.PurchaseOrders;
using GoodHamburger.Shared.Models.PurchaseOrders;

namespace GoodHamburger.Portal.Services.Orders;

public class OrderService
{
    private readonly HttpClient _http;

    public OrderService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Obtém todos os pedidos.
    /// </summary>
    /// <exception cref="HttpRequestException">401 Unauthorized, 403 Forbidden.</exception>
    public Task<List<Order>> GetAllAsync() =>
        _http.GetFromJsonAsync<List<Order>>("api/orders")!;

    /// <summary>
    /// Obtém um pedido pelo ID.
    /// </summary>
    /// <param name="id">ID do pedido.</param>
    /// <exception cref="HttpRequestException">401 Unauthorized, 403 Forbidden, 404 Not Found.</exception>
    public Task<Order> GetByIdAsync(Guid id) =>
        _http.GetFromJsonAsync<Order>($"api/orders/{id}")!;

    /// <summary>
    /// Cria um novo pedido.
    /// </summary>
    /// <param name="dto">Dados do pedido.</param>
    /// <exception cref="HttpRequestException">400 Bad Request, 401 Unauthorized, 403 Forbidden.</exception>
    public async Task<Order> CreateAsync(CreateOrderDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/orders", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Order>())!;
    }

    /// <summary>
    /// Atualiza o status de um pedido.
    /// </summary>
    /// <param name="dto">DTO com ID e novo status.</param>
    /// <exception cref="HttpRequestException">400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found.</exception>
    public Task UpdateStatusAsync(UpdateOrderStatusDto dto) =>
        _http.PutAsJsonAsync("api/orders/status", dto);

    /// <summary>
    /// Busca pedidos por critérios opcionais.
    /// </summary>
    /// <param name="customerId">ID do cliente (opcional).</param>
    /// <param name="orderDate">Data do pedido (opcional).</param>
    /// <exception cref="HttpRequestException">401 Unauthorized, 403 Forbidden.</exception>
    public async Task<List<Order>> FindAsync(Guid? customerId = null, DateTime? orderDate = null)
    {
        var query = new List<string>();
        if (customerId.HasValue) query.Add($"CustomerId={customerId}");
        if (orderDate.HasValue) query.Add($"OrderDate={orderDate.Value:O}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
        return (await _http.GetFromJsonAsync<List<Order>>($"api/orders/search{qs}"))!;
    }
}