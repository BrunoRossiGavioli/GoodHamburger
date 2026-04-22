using GoodHamburger.API.Entities.Products;
using GoodHamburger.API.Extensions.Entities.Products;

namespace GoodHamburger.Tests.Extensions;

public class ProductPriceEntityExtensionsTests
{
    // ── ValidAt (IEnumerable) ─────────────────────────────────────────────────

    [Fact]
    public void ValidAt_PriceActiveAtDate_IncludesPrice()
    {
        var price = BuildPrice(start: new DateTime(2025, 1, 1), end: null);
        var result = new[] { price }.ValidAt(new DateTime(2025, 6, 1));
        Assert.Single(result);
    }

    [Fact]
    public void ValidAt_PriceNotStartedYet_ExcludesPrice()
    {
        var price = BuildPrice(start: new DateTime(2026, 1, 1), end: null);
        var result = new[] { price }.ValidAt(new DateTime(2025, 6, 1));
        Assert.Empty(result);
    }

    [Fact]
    public void ValidAt_PriceAlreadyExpired_ExcludesPrice()
    {
        var price = BuildPrice(start: new DateTime(2024, 1, 1), end: new DateTime(2024, 12, 31));
        var result = new[] { price }.ValidAt(new DateTime(2025, 6, 1));
        Assert.Empty(result);
    }

    [Fact]
    public void ValidAt_PriceExpiresExactlyAtDate_IncludesPrice()
    {
        var date = new DateTime(2025, 6, 1);
        var price = BuildPrice(start: new DateTime(2025, 1, 1), end: date);
        var result = new[] { price }.ValidAt(date);
        Assert.Single(result);
    }

    [Fact]
    public void ValidAt_MultipleOverlappingPrices_ReturnsOnlyActive()
    {
        var checkDate = new DateTime(2025, 6, 15);
        var active = BuildPrice(start: new DateTime(2025, 6, 1), end: null);
        var expired = BuildPrice(start: new DateTime(2025, 1, 1), end: new DateTime(2025, 5, 31));
        var future = BuildPrice(start: new DateTime(2025, 7, 1), end: null);

        var result = new[] { active, expired, future }.ValidAt(checkDate).ToList();

        Assert.Single(result);
        Assert.Same(active, result[0]);
    }

    // ── GetCurrentPrice (IEnumerable) ─────────────────────────────────────────

    [Fact]
    public void GetCurrentPrice_SingleActivePrice_ReturnsIt()
    {
        var productId = Guid.NewGuid();
        var price = BuildPrice(productId: productId, start: DateTime.UtcNow.AddDays(-1), end: null);

        var result = new[] { price }.GetCurrentPrice(productId);

        Assert.Equal(price, result);
    }

    [Fact]
    public void GetCurrentPrice_MultiplePrices_ReturnsLatestStartDate()
    {
        var productId = Guid.NewGuid();
        var older = BuildPrice(productId: productId, start: new DateTime(2024, 1, 1), end: null, value: 5.00m);
        var newer = BuildPrice(productId: productId, start: new DateTime(2025, 1, 1), end: null, value: 6.00m);

        var result = new[] { older, newer }.GetCurrentPrice(productId, new DateTime(2025, 6, 1));

        Assert.Equal(6.00m, result.Value);
    }

    [Fact]
    public void GetCurrentPrice_NoValidPrice_ThrowsInvalidOperationException()
    {
        var productId = Guid.NewGuid();
        var futurePrice = BuildPrice(productId: productId, start: new DateTime(2030, 1, 1), end: null);

        Assert.Throws<InvalidOperationException>(() =>
            new[] { futurePrice }.GetCurrentPrice(productId, DateTime.UtcNow));
    }

    [Fact]
    public void GetCurrentPrice_PriceForDifferentProduct_ThrowsInvalidOperationException()
    {
        var productId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var price = BuildPrice(productId: otherId, start: DateTime.UtcNow.AddDays(-1), end: null);

        Assert.Throws<InvalidOperationException>(() =>
            new[] { price }.GetCurrentPrice(productId));
    }

    // ── ProductEntityExtensions.GetCurrentPrice ────────────────────────────────

    [Fact]
    public void ProductEntityExtensions_GetCurrentPrice_ReturnsActivePrice()
    {
        var productId = Guid.NewGuid();
        var product = new ProductEntity
        {
            Id = productId,
            Name = "X Burger",
            Prices =
            [
                BuildPrice(productId: productId, start: DateTime.UtcNow.AddDays(-1), end: null, value: 5.00m)
            ]
        };

        var result = product.GetCurrentPrice();

        Assert.Equal(5.00m, result.Value);
    }

    [Fact]
    public void ProductEntityExtensions_GetCurrentPrice_WithDateRef_ReturnsCorrectPrice()
    {
        var productId = Guid.NewGuid();
        var dateRef = new DateTime(2025, 6, 1);
        var product = new ProductEntity
        {
            Id = productId,
            Name = "X Egg",
            Prices =
            [
                BuildPrice(productId: productId, start: new DateTime(2024, 1, 1), end: new DateTime(2025, 5, 31), value: 4.00m),
                BuildPrice(productId: productId, start: new DateTime(2025, 6, 1), end: null, value: 4.50m)
            ]
        };

        var result = product.GetCurrentPrice(dateRef);

        Assert.Equal(4.50m, result.Value);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ProductPriceEntity BuildPrice(
        Guid? productId = null,
        DateTime? start = null,
        DateTime? end = null,
        decimal value = 5.00m) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = productId ?? Guid.NewGuid(),
        Value = value,
        StartDate = start ?? DateTime.UtcNow.AddDays(-1),
        EndDate = end,
        Reason = "Test"
    };
}
