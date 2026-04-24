using GoodHamburger.Portal.Services.Auth;
using GoodHamburger.Shared.DTOs.Products;
using GoodHamburger.Shared.Models.Products;

namespace GoodHamburger.Portal.Services.Products;

public class ProductService
{
    private readonly ApiHttpClient _http;

    public ProductService(ApiHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Obtém todos os produtos ativos.
    /// </summary>
    /// <exception cref="HttpRequestException">401 Unauthorized, 403 Forbidden.</exception>
    public Task<List<Product>> GetAllAsync() =>
        _http.GetAsync<List<Product>>("api/products")!;

    /// <summary>
    /// Obtém um produto pelo ID.
    /// </summary>
    /// <param name="id">ID do produto.</param>
    /// <exception cref="HttpRequestException">401 Unauthorized, 403 Forbidden, 404 Not Found.</exception>
    public Task<Product> GetByIdAsync(Guid id) =>
        _http.GetAsync<Product>($"api/products/{id}")!;

    /// <summary>
    /// Cria um novo produto.
    /// </summary>
    /// <param name="dto">Dados do produto.</param>
    /// <exception cref="HttpRequestException">400 Bad Request, 401 Unauthorized, 403 Forbidden.</exception>
    public async Task<Product> CreateAsync(CreateProductDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/products", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Product>())!;
    }

    /// <summary>
    /// Atualiza um produto existente.
    /// </summary>
    /// <param name="dto">Dados atualizados do produto.</param>
    /// <exception cref="HttpRequestException">400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found.</exception>
    public async Task<Product> UpdateAsync(UpdateProductDto dto)
    {
        var response = await _http.PutAsJsonAsync("api/products", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Product>())!;
    }

    /// <summary>
    /// Remove um produto pelo ID.
    /// </summary>
    /// <param name="id">ID do produto.</param>
    /// <exception cref="HttpRequestException">400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found.</exception>
    public Task DeleteAsync(Guid id) =>
        _http.DeleteAsync($"api/products/{id}");

    /// <summary>
    /// Busca produtos pelo nome.
    /// </summary>
    /// <param name="name">Nome ou parte do nome do produto (opcional).</param>
    /// <exception cref="HttpRequestException">401 Unauthorized, 403 Forbidden.</exception>
    public async Task<List<Product>> FindAsync(string? name = null)
    {
        var qs = !string.IsNullOrWhiteSpace(name) ? $"?Name={Uri.EscapeDataString(name)}" : "";
        return (await _http.GetAsync<List<Product>>($"api/products/search{qs}"))!;
    }

    /// <summary>
    /// Atualiza o estado ativo/inativo de um produto.
    /// </summary>
    /// <param name="dto">DTO com ID e novo estado.</param>
    /// <exception cref="HttpRequestException">400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found.</exception>
    public Task UpdateActiveStateAsync(UpdateProductActiveStateDto dto) =>
        _http.PutAsJsonAsync("api/products/activeState", dto);

    /// <summary>
    /// Adiciona um novo preço ao histórico de preços de um produto.
    /// </summary>
    /// <param name="dto">Dados do preço.</param>
    /// <exception cref="HttpRequestException">400 Bad Request, 401 Unauthorized, 403 Forbidden.</exception>
    public async Task<ProductPrice> AddPriceAsync(CreateProductPriceDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/product-prices", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductPrice>())!;
    }

    /// <summary>
    /// Remove um preço do histórico.
    /// </summary>
    /// <param name="id">ID do preço.</param>
    /// <exception cref="HttpRequestException">400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found.</exception>
    public Task DeletePriceAsync(Guid id) =>
        _http.DeleteAsync($"api/product-prices/{id}");

    /// <summary>
    /// Busca histórico de preços de um produto.
    /// </summary>
    /// <param name="productId">ID do produto.</param>
    /// <param name="startDate">Data inicial para filtro (opcional).</param>
    /// <exception cref="HttpRequestException">401 Unauthorized, 403 Forbidden.</exception>
    public async Task<List<ProductPrice>> SearchPricesAsync(Guid productId, DateTime? startDate = null)
    {
        var query = new List<string> { $"ProductId={productId}" };
        if (startDate.HasValue) query.Add($"StartDate={startDate.Value:O}");
        var qs = "?" + string.Join("&", query);
        return (await _http.GetAsync<List<ProductPrice>>($"api/product-prices/search{qs}"))!;
    }
}