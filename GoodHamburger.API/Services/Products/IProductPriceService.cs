using GoodHamburger.Shared.DTOs.Products;
using GoodHamburger.Shared.Models.Products;

namespace GoodHamburger.API.Services.Products;

public interface IProductPriceService
{
    Task<ProductPrice?> GetAsync(GetProductPriceDto dto);
    Task<IEnumerable<ProductPrice>> FindAsync(FindProductPriceDto dto);

    Task<ProductPrice> CreateAsync(CreateProductPriceDto dto);
    Task DeleteAsync(UpdateProductPriceDto dto);
}
