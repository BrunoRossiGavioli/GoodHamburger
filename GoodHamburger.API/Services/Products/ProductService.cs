using GoodHamburger.Shared.DTOs.Products;
using GoodHamburger.Shared.Models.Products;

namespace GoodHamburger.API.Services.Products;

//TODO: Implementar o serviço de produtos
public class ProductService : IProductService
{
    public Task<Product> CreateAsync(CreateProductDto dto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(UpdateProductDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Product>> FindAsync(FindProductDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Product>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Product?> GetAsync(GetProductDto dto)
    {
        throw new NotImplementedException();
    }

    public Task UpdateActiveState(UpdateProductActiveStateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<Product> UpdateAsync(UpdateProductDto dto)
    {
        throw new NotImplementedException();
    }
}
