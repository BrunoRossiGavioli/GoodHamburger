using GoodHamburger.Shared.DTOs.Products;
using GoodHamburger.Shared.Models.Products;

namespace GoodHamburger.API.Services.Products;

//TODO: Implementar o serviço de preços dos produtos
public class ProductPriceService : IProductPriceService
{
    public Task<ProductPrice> CreateAsync(CreateProductPriceDto dto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(UpdateProductPriceDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ProductPrice>> FindAsync(FindProductPriceDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ProductPrice?> GetAsync(GetProductPriceDto dto)
    {
        throw new NotImplementedException();
    }
}
