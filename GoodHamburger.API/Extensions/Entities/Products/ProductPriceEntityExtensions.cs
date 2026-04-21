using GoodHamburger.API.Entities.Products;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.API.Extensions.Entities.Products;

public static class ProductPriceEntityExtensions
{
    /// <summary>
    /// Filtra os preços válidos para uma data específica
    /// </summary>
    /// <param name="query">Query de preços</param>
    /// <param name="date">Data para verificar vigência (default = data atual)</param>
    /// <returns>Preços ativos na data informada</returns>
    public static IQueryable<ProductPriceEntity> ValidAt(this IQueryable<ProductPriceEntity> query, DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;

        return query.Where(p => p.StartDate <= targetDate &&
                               (p.EndDate == null || p.EndDate >= targetDate));
    }

    /// <summary>
    /// Obtém o preço atual de um produto em uma data específica
    /// </summary>
    /// <param name="query">Query de preços</param>
    /// <param name="productId">ID do produto</param>
    /// <param name="date">Data para verificar vigência (default = data atual)</param>
    /// <returns>Preço ativo ou null se não encontrado</returns>
    public static async Task<ProductPriceEntity> GetCurrentPriceAsync(
        this IQueryable<ProductPriceEntity> query,
        Guid productId,
        DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;

        return await query
            .Where(p => p.ProductId == productId)
            .ValidAt(targetDate)
            .OrderByDescending(p => p.StartDate)
            .FirstAsync();
    }

    /// <summary>
    /// Filtra os preços válidos para uma data específica
    /// </summary>
    /// <param name="query">Query de preços</param>
    /// <param name="date">Data para verificar vigência (default = data atual)</param>
    /// <returns>Preços ativos na data informada</returns>
    public static IEnumerable<ProductPriceEntity> ValidAt(this IEnumerable<ProductPriceEntity> query, DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;

        return query.Where(p => p.StartDate <= targetDate &&
                               (p.EndDate == null || p.EndDate >= targetDate));
    }

    /// <summary>
    /// Obtém o preço atual de um produto em uma data específica
    /// </summary>
    /// <param name="query">Query de preços</param>
    /// <param name="productId">ID do produto</param>
    /// <param name="date">Data para verificar vigência (default = data atual)</param>
    /// <returns>Preço ativo ou null se não encontrado</returns>
    public static ProductPriceEntity GetCurrentPrice(
        this IEnumerable<ProductPriceEntity> query,
        Guid productId,
        DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;

        return query
            .Where(p => p.ProductId == productId)
            .ValidAt(targetDate)
            .OrderByDescending(p => p.StartDate)
            .First();
    }
}