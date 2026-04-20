using GoodHamburger.Shared.DTOs.Products;
using GoodHamburger.Shared.Models.Products;

namespace GoodHamburger.API.Services.Products;

public interface IProductService
{
    Task<Product?> GetAsync(GetProductDto dto);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> FindAsync(FindProductDto dto);

    Task<Product> CreateAsync(CreateProductDto dto);
    Task<Product> UpdateAsync(UpdateProductDto dto);
    Task UpdateActiveState(UpdateProductActiveStateDto dto);
    Task DeleteAsync(UpdateProductDto dto);
}
