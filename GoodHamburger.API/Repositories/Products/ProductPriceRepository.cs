using GoodHamburger.API.Data;
using GoodHamburger.API.Entities.Products;
using System.Linq.Expressions;

namespace GoodHamburger.API.Repositories.Products
{
    public class ProductPriceRepository : IProductPriceRepository
    {
        private readonly AppDbContext _context;

        public ProductPriceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductPriceEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ProductPrices.FindAsync([id], cancellationToken);
        }

        public async Task<IEnumerable<ProductPriceEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context.ProductPrices);
        }

        public async Task<IEnumerable<ProductPriceEntity>> FindAsync(Expression<Func<ProductPriceEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context.ProductPrices.Where(predicate));
        }

        public async Task<ProductPriceEntity> AddAsync(ProductPriceEntity entity, CancellationToken cancellationToken = default)
        {
            await _context.ProductPrices.AddAsync(entity, cancellationToken);
            return entity;
        }

        public Task UpdateAsync(ProductPriceEntity entity, CancellationToken cancellationToken = default)
        {
            _context.ProductPrices.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ProductPriceEntity entity, CancellationToken cancellationToken = default)
        {
            _context.ProductPrices.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Expression<Func<ProductPriceEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_context.ProductPrices.Any(predicate));
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
