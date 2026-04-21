using GoodHamburger.API.Entities.Products;
using GoodHamburger.API.Extensions.Entities;
using GoodHamburger.API.Repositories.Products;
using GoodHamburger.Shared.DTOs.Products;
using GoodHamburger.Shared.Models.Products;

namespace GoodHamburger.API.Services.Products;

public class ProductPriceService : IProductPriceService
{
    private readonly IProductPriceRepository _productPriceRepository;
    private readonly IProductRepository _productRepository;

    public ProductPriceService(IProductPriceRepository productPriceRepository, IProductRepository productRepository)
    {
        _productPriceRepository = productPriceRepository ?? throw new ArgumentNullException(nameof(productPriceRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<IEnumerable<ProductPrice>> FindAsync(FindProductPriceDto dto)
    {
        var entities = await _productPriceRepository.FindAsync(e =>
            e.ProductId == dto.ProductId &&
            (dto.StartDate == null || e.StartDate == dto.StartDate));
        return entities.Select(e => e.MapEntityToModel());
    }

    public async Task<ProductPrice> CreateAsync(CreateProductPriceDto dto)
    {
        var productExists = await _productRepository.ExistsAsync(p => p.Id == dto.ProductId);
        if (!productExists)
            throw new InvalidOperationException("Product not found.");

        var productPriceEntity = await _productPriceRepository.AddAsync(new ProductPriceEntity
        {
            ProductId = dto.ProductId,
            Value = dto.Value,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason.Trim(),
        });
        await _productPriceRepository.SaveChangesAsync();
        return productPriceEntity.MapEntityToModel();
    }

    public async Task DeleteAsync(DeleteProductPriceDto dto)
    {
        var productPriceEntity = await _productPriceRepository.GetByIdAsync(dto.Id);
        if (productPriceEntity == null)
            throw new InvalidOperationException("Product price not found.");

        //TODO: Verificar se o preço já foi utilizada em algum pedido e, se sim, lançar uma exceção em vez de deletar.

        await _productPriceRepository.DeleteAsync(productPriceEntity);
        await _productPriceRepository.SaveChangesAsync();
    }
}

