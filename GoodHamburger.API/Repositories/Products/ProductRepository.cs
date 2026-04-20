using GoodHamburger.API.Data;
using GoodHamburger.API.Entities.Products;
using System.Linq.Expressions;

namespace GoodHamburger.API.Repositories.Products
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products.FindAsync([id], cancellationToken);
        }

        public async Task<IEnumerable<ProductEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context.Products);
        }

        public async Task<IEnumerable<ProductEntity>> FindAsync(Expression<Func<ProductEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context.Products.Where(predicate));
        }

        public async Task<ProductEntity> AddAsync(ProductEntity entity, CancellationToken cancellationToken = default)
        {
            await _context.Products.AddAsync(entity, cancellationToken);
            return entity;
        }

        public Task UpdateAsync(ProductEntity entity, CancellationToken cancellationToken = default)
        {
            _context.Products.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ProductEntity entity, CancellationToken cancellationToken = default)
        {
            _context.Products.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Expression<Func<ProductEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_context.Products.Any(predicate));
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
