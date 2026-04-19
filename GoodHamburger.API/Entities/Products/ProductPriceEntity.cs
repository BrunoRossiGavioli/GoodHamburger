namespace GoodHamburger.API.Entities.Products
{
    public class ProductPriceEntity
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public decimal Value { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Reason { get; set; } = default!;

        public ProductEntity Product { get; set; } = default!;
    }
}
