using GoodHamburger.API.Entities.Products;
using GoodHamburger.API.Repositories.Products;
using GoodHamburger.API.Services.Products;
using GoodHamburger.Shared.DTOs.Products;
using Moq;
using System.Linq.Expressions;

namespace GoodHamburger.Tests.Services;

public class ProductPriceServiceTests
{
    private readonly Mock<IProductPriceRepository> _priceRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly ProductPriceService _sut;

    public ProductPriceServiceTests()
    {
        _sut = new ProductPriceService(_priceRepoMock.Object, _productRepoMock.Object);
    }

    [Fact]
    public void Constructor_NullPriceRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ProductPriceService(null!, _productRepoMock.Object));
    }

    [Fact]
    public void Constructor_NullProductRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ProductPriceService(_priceRepoMock.Object, null!));
    }

    // ── FindAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task FindAsync_ByProductId_ReturnsMappedPrices()
    {
        var productId = Guid.NewGuid();
        var entities = new List<ProductPriceEntity> { BuildPrice(productId: productId), BuildPrice(productId: productId) };
        _priceRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entities);

        var result = await _sut.FindAsync(new FindProductPriceDto(productId));

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task FindAsync_ByProductIdAndStartDate_ReturnsMappedPrices()
    {
        var productId = Guid.NewGuid();
        var startDate = new DateTime(2025, 1, 1);
        var entities = new List<ProductPriceEntity> { BuildPrice(productId: productId, startDate: startDate) };
        _priceRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entities);

        var result = await _sut.FindAsync(new FindProductPriceDto(productId, startDate));

        Assert.Single(result);
        Assert.Equal(startDate, result.First().StartDate);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ExistingProduct_CreatesPriceWithTrimmedReason()
    {
        var productId = Guid.NewGuid();
        var dto = new CreateProductPriceDto(productId, 6.00m, new DateTime(2025, 6, 1), null, " Reajuste anual ");

        _productRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<ProductEntity, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

        ProductPriceEntity? capturedEntity = null;
        _priceRepoMock.Setup(r => r.AddAsync(It.IsAny<ProductPriceEntity>(), It.IsAny<CancellationToken>()))
                      .Callback<ProductPriceEntity, CancellationToken>((e, _) => capturedEntity = e)
                      .ReturnsAsync((ProductPriceEntity e, CancellationToken _) => e);
        _priceRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.CreateAsync(dto);

        Assert.NotNull(capturedEntity);
        Assert.Equal("Reajuste anual", capturedEntity!.Reason);
        Assert.Equal(6.00m, capturedEntity.Value);
        Assert.Equal(productId, capturedEntity.ProductId);
    }

    [Fact]
    public async Task CreateAsync_ProductNotFound_ThrowsInvalidOperationException()
    {
        _productRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<ProductEntity, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(new CreateProductPriceDto(Guid.NewGuid(), 5.00m, Datetime.Now, null, "Reason")));
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingPrice_DeletesSuccessfully()
    {
        var id = Guid.NewGuid();
        var entity = BuildPrice(id: id);
        _priceRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _priceRepoMock.Setup(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _priceRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.DeleteAsync(new DeleteProductPriceDto(id));

        _priceRepoMock.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        _priceRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_PriceNotFound_ThrowsInvalidOperationException()
    {
        _priceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ProductPriceEntity?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.DeleteAsync(new DeleteProductPriceDto(Guid.NewGuid())));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ProductPriceEntity BuildPrice(Guid? id = null, Guid? productId = null, DateTime? startDate = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        ProductId = productId ?? Guid.NewGuid(),
        Value = 5.00m,
        StartDate = startDate ?? Datetime.Now.AddDays(-1),
        EndDate = null,
        Reason = "Preço inicial"
    };
}
