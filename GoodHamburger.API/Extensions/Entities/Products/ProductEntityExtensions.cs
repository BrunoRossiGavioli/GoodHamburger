using GoodHamburger.API.Entities.Products;

namespace GoodHamburger.API.Extensions.Entities.Products
{
    public static class ProductEntityExtensions
    {
        /// <summary>
        /// Obtém o preço atual de um produto em uma data específica
        /// </summary>
        /// <param name="product">O produto</param>
        /// <param name="date">Data para verificar vigência (default = data atual)</param>
        /// <returns>Preço ativo ou null se não encontrado</returns>
        public static ProductPriceEntity GetCurrentPrice(this ProductEntity product,
            DateTime? date = null)
        {
            var targetDate = date ?? DateTime.UtcNow;

            return product.Prices.GetCurrentPrice(product.Id, targetDate);
        }
    }
}
