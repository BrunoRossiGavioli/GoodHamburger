using GoodHamburger.API.Entities.Products;

namespace GoodHamburger.API.Repositories.Products
{
    public interface IProductRepository : IRepository<ProductEntity, Guid>
    {
    }
}
