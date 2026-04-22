using GoodHamburger.API.Entities.Products;
using GoodHamburger.API.Repositories.Products;
using GoodHamburger.API.Services.Products;
using GoodHamburger.Shared.DTOs.Products;
using GoodHamburger.Shared.Enums;
using Moq;
using System.Linq.Expressions;

namespace GoodHamburger.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repoMock = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_repoMock.Object);
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ProductService(null!));
    }

    // ── GetAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_ExistingProduct_ReturnsProduct()
    {
        var id = Guid.NewGuid();
        var entity = BuildProduct(id);
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var result = await _sut.GetAsync(new GetProductDto(id));

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("X Burger", result.Name);
        Assert.Equal(5.00m, result.Price);
        Assert.Equal(ProductType.Sandwich, result.Type);
    }

    [Fact]
    public async Task GetAsync_NonExistingProduct_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ProductEntity?)null);

        var result = await _sut.GetAsync(new GetProductDto(Guid.NewGuid()));

        Assert.Null(result);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllProductsMapped()
    {
        var entities = new List<ProductEntity> { BuildProduct(), BuildProduct() };
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    // ── FindAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task FindAsync_MatchingProducts_ReturnsMappedResults()
    {
        var entities = new List<ProductEntity> { BuildProduct() };
        _repoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductEntity, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entities);

        var result = await _sut.FindAsync(new FindProductDto("Burger"));

        Assert.Single(result);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_TrimsWhitespace_AndPersistsCorrectFields()
    {
        // The service sets Prices AFTER AddAsync; SaveChangesAsync simulates EF linking ProductId.
        ProductEntity? capturedEntity = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
                 .Callback<ProductEntity, CancellationToken>((e, _) => capturedEntity = e)
                 .ReturnsAsync((ProductEntity e, CancellationToken _) => e);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback<CancellationToken>(_ =>
                 {
                     // Simulate EF Core populating ProductId on owned prices
                     if (capturedEntity?.Prices is not null)
                         foreach (var p in capturedEntity.Prices.Where(p => p.ProductId == Guid.Empty))
                             p.ProductId = capturedEntity.Id;
                 })
                 .ReturnsAsync(1);

        var result = await _sut.CreateAsync(new CreateProductDto(" X Burger ", " Hamburguer clássico ", ProductType.Sandwich, 5.00m));

        Assert.NotNull(result);
        _repoMock.Verify(r => r.AddAsync(It.Is<ProductEntity>(e =>
            e.Name == "X Burger" && e.Description == "Hamburguer clássico" && e.Type == ProductType.Sandwich),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SetsInitialPriceWithReason()
    {
        ProductEntity? capturedEntity = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
                 .Callback<ProductEntity, CancellationToken>((e, _) => capturedEntity = e)
                 .ReturnsAsync((ProductEntity e, CancellationToken _) => e);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback<CancellationToken>(_ =>
                 {
                     if (capturedEntity?.Prices is not null)
                         foreach (var p in capturedEntity.Prices.Where(p => p.ProductId == Guid.Empty))
                             p.ProductId = capturedEntity.Id;
                 })
                 .ReturnsAsync(1);

        await _sut.CreateAsync(new CreateProductDto("X Egg", "Com ovo", ProductType.Sandwich, 4.50m));

        Assert.NotNull(capturedEntity);
        var price = Assert.Single(capturedEntity!.Prices);
        Assert.Equal("Preço inicial", price.Reason);
        Assert.Equal(4.50m, price.Value);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingProduct_UpdatesNameAndDescription()
    {
        var id = Guid.NewGuid();
        var entity = BuildProduct(id, name: "Updated");
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(new UpdateProductDto(id, " Updated ", " New Desc "));

        Assert.Equal("Updated", result.Name);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ProductNotFound_ThrowsInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ProductEntity?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.UpdateAsync(new UpdateProductDto(Guid.NewGuid(), "X", "X")));
    }

    // ── UpdateActiveState ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateActiveState_ExistingProduct_SetsIsActiveFlag()
    {
        var id = Guid.NewGuid();
        var entity = BuildProduct(id);
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.UpdateActiveState(new UpdateProductActiveStateDto(id, false));

        Assert.False(entity.IsActive);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateActiveState_ProductNotFound_ThrowsInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ProductEntity?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.UpdateActiveState(new UpdateProductActiveStateDto(Guid.NewGuid(), true)));
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingProduct_DeletesSuccessfully()
    {
        var id = Guid.NewGuid();
        var entity = BuildProduct(id);
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.DeleteAsync(new UpdateProductDto(id, "X", "X"));

        _repoMock.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ProductNotFound_ThrowsInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ProductEntity?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.DeleteAsync(new UpdateProductDto(Guid.NewGuid(), "X", "X")));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    internal static ProductEntity BuildProduct(Guid? id = null, string name = "X Burger", decimal price = 5.00m, ProductType type = ProductType.Sandwich)
    {
        var productId = id ?? Guid.NewGuid();
        return new ProductEntity
        {
            Id = productId,
            Name = name,
            Description = "Hamburguer clássico",
            Type = type,
            IsActive = true,
            Prices =
            [
                new ProductPriceEntity
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Value = price,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = null,
                    Reason = "Preço inicial"
                }
            ]
        };
    }
}
