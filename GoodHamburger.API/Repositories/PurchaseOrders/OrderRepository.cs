using GoodHamburger.API.Data;
using GoodHamburger.API.Entities.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
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
            return await _context.Orders.Where(o => o.Id == id).Include(o => o.Customer).Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Prices).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<OrderEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Orders.Include(o => o.Customer).Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Prices).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<OrderEntity>> FindAsync(Expression<Func<OrderEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Orders.Where(predicate).Include(o => o.Customer).Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Prices).ToListAsync(cancellationToken);
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

        public async Task<bool> ExistsAsync(Expression<Func<OrderEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Orders.AnyAsync(predicate, cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
