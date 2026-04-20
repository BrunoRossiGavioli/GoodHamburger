using GoodHamburger.API.Repositories.Products;

namespace GoodHamburger.API.UnitOfWorkers;

public interface IProductWork : IUnitOfWork
{
    IProductRepository Products { get; }
    IProductPriceRepository ProductPrices { get; }
}
