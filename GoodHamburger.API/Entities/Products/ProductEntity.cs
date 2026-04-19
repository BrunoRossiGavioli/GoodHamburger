using GoodHamburger.Shared.Enums;

namespace GoodHamburger.API.Entities.Products
{
    public class ProductEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public ProductType Type { get; set; }

        public ICollection<ProductPriceEntity> Prices { get; set; } = default!;
    }
}
