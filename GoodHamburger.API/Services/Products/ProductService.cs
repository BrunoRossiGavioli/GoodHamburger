using GoodHamburger.API.Entities.Products;
using GoodHamburger.API.Extensions.Entities;
using GoodHamburger.API.Repositories.Products;
using GoodHamburger.Shared.DTOs.Products;
using GoodHamburger.Shared.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.API.Services.Products;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<IEnumerable<Product>> FindAsync(FindProductDto dto)
    {
        var entities = await _productRepository.FindAsync(e =>
            (dto.Name != null && EF.Functions.Like(e.Name, $"%{dto.Name}%")) ||
            (dto.Type != null && e.Type.ToString() == dto.Type));
        return entities.Select(p => p.MapEntityToModel());
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return (await _productRepository.GetAllAsync()).Select(p => p.MapEntityToModel());
    }

    public async Task<Product?> GetAsync(GetProductDto dto)
    {
        var productEntity = await _productRepository.GetByIdAsync(dto.Id);
        if (productEntity is null)
            return null;

        return productEntity.MapEntityToModel();
    }

    public async Task<Product> CreateAsync(CreateProductDto dto)
    {
        var productEntity = await _productRepository.AddAsync(new ProductEntity
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Type = dto.Type,
        });

        await _productRepository.SaveChangesAsync();
        return productEntity.MapEntityToModel();
    }

    public async Task<Product> UpdateAsync(UpdateProductDto dto)
    {
        var productEntity = await _productRepository.GetByIdAsync(dto.Id) ?? throw new InvalidOperationException("Product not found.");

        await _productRepository.UpdateAsync(new ProductEntity
        {
            Id = dto.Id,
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Type = productEntity.Type
        });
        await _productRepository.SaveChangesAsync();

        return await GetAsync(new GetProductDto(dto.Id)) ?? throw new InvalidOperationException("Failed to retrieve the updated product.");
    }

    public async Task UpdateActiveState(UpdateProductActiveStateDto dto)
    {
        //TODO: UpdateProductActiveState
    }

    public async Task DeleteAsync(UpdateProductDto dto)
    {
        var productEntity = await _productRepository.GetByIdAsync(dto.Id) ?? throw new InvalidOperationException("Product not found.");
        //TODO: Lançar exceção caso o produto esteja associado a um pedido ativo

        await _productRepository.DeleteAsync(productEntity);
        await _productRepository.SaveChangesAsync();
    }
}
