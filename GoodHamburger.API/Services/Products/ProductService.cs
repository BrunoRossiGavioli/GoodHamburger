using GoodHamburger.API.Entities.Products;
using GoodHamburger.API.Extensions.Entities;
using GoodHamburger.API.Repositories.Products;
using GoodHamburger.Shared.DTOs.Products;
using GoodHamburger.Shared.Models.Products;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

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
        if(dto.Name is null)
            return await GetAllAsync();

        var entities = await _productRepository.FindAsync(e => dto.Name != null && EF.Functions.Like(e.Name, $"%{dto.Name}%"));
        return entities.Select(p => p.MapEntityToModel());
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return (await _productRepository.GetAllAsync()).Select(p => p.MapEntityToModel());
    }

    public async Task<Product?> GetAsync(GetProductDto dto)
    {
        var productEntity = await _productRepository.GetByIdAsync(dto.Id);
        return productEntity?.MapEntityToModel();
    }

    public async Task<Product> CreateAsync(CreateProductDto dto)
    {
        var productEntity = await _productRepository.AddAsync(new()
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Type = dto.Type,
        });

        productEntity.Prices = [
        new()
        {
            Reason = "Preço inicial",
            Value = dto.Price,
            StartDate = DateTime.UtcNow
        }];

        await _productRepository.SaveChangesAsync();
        return productEntity.MapEntityToModel();
    }

    public async Task<Product> UpdateAsync(UpdateProductDto dto)
    {
        var productEntity = await _productRepository.GetByIdAsync(dto.Id) ?? throw new InvalidOperationException("Product not found.");
        productEntity.Name = dto.Name.Trim();
        productEntity.Description = dto.Description.Trim();

        await _productRepository.SaveChangesAsync();

        return productEntity.MapEntityToModel();
    }

    public async Task UpdateActiveState(UpdateProductActiveStateDto dto)
    {
        var productEntity = await _productRepository.GetByIdAsync(dto.Id) ?? throw new InvalidOperationException("Product not found.");

        productEntity.IsActive = dto.IsActive;
        await _productRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(UpdateProductDto dto)
    {
        try
        {
            var productEntity = await _productRepository.GetByIdAsync(dto.Id) ?? throw new InvalidOperationException("Product not found.");
            await _productRepository.DeleteAsync(productEntity);
            await _productRepository.SaveChangesAsync();
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("Product can't be removed");
        }
    }
}
