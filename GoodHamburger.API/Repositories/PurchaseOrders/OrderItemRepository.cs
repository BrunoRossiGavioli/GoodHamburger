using GoodHamburger.API.Data;
using GoodHamburger.API.Entities.PurchaseOrders;
using System.Linq.Expressions;

namespace GoodHamburger.API.Repositories.PurchaseOrders
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly AppDbContext _context;

        public OrderItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<OrderItemEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<OrderItemEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context.OrderItems);
        }

        public async Task<IEnumerable<OrderItemEntity>> FindAsync(Expression<Func<OrderItemEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context.OrderItems.Where(predicate));
        }

        public async Task<OrderItemEntity> AddAsync(OrderItemEntity entity, CancellationToken cancellationToken = default)
        {
            await _context.OrderItems.AddAsync(entity, cancellationToken);
            return entity;
        }

        public Task UpdateAsync(OrderItemEntity entity, CancellationToken cancellationToken = default)
        {
            _context.OrderItems.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(OrderItemEntity entity, CancellationToken cancellationToken = default)
        {
            _context.OrderItems.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Expression<Func<OrderItemEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_context.OrderItems.Any(predicate));
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
