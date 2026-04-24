using GoodHamburger.API.Data;
using GoodHamburger.API.Entities.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
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
            return await _context.OrderItems.Include(oi => oi.Order).Include(oi => oi.Product).ThenInclude(p => p.Prices).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<OrderItemEntity>> FindAsync(Expression<Func<OrderItemEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.OrderItems.Where(predicate).Include(oi => oi.Order).Include(oi => oi.Product).ThenInclude(p => p.Prices).ToListAsync(cancellationToken);
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

        public async Task<bool> ExistsAsync(Expression<Func<OrderItemEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.OrderItems.AnyAsync(predicate));
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
