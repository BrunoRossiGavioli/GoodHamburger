using GoodHamburger.API.Data;
using GoodHamburger.API.Entities.PurchaseOrders;
using System.Linq.Expressions;

namespace GoodHamburger.API.Repositories.PurchaseOrders
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Orders.FindAsync([id], cancellationToken);
        }

        public async Task<IEnumerable<OrderEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context.Orders);
        }

        public async Task<IEnumerable<OrderEntity>> FindAsync(Expression<Func<OrderEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context.Orders.Where(predicate));
        }

        public async Task<OrderEntity> AddAsync(OrderEntity entity, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(entity, cancellationToken);
            return entity;
        }

        public Task UpdateAsync(OrderEntity entity, CancellationToken cancellationToken = default)
        {
            _context.Orders.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(OrderEntity entity, CancellationToken cancellationToken = default)
        {
            _context.Orders.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Expression<Func<OrderEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_context.Orders.Any(predicate));
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
