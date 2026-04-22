using GoodHamburger.API.Data;
using GoodHamburger.API.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GoodHamburger.API.Repositories.Customers
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Customers.Where(c => c.Id == id).Include(c => c.Orders).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<CustomerEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Customers.Include(c => c.Orders).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CustomerEntity>> FindAsync(Expression<Func<CustomerEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Customers.Where(predicate).Include(c => c.Orders).ToListAsync();
        }

        public async Task<CustomerEntity> AddAsync(CustomerEntity entity, CancellationToken cancellationToken = default)
        {
            await _context.Customers.AddAsync(entity, cancellationToken);
            return entity;
        }

        public Task UpdateAsync(CustomerEntity entity, CancellationToken cancellationToken = default)
        {
            _context.Customers.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(CustomerEntity entity, CancellationToken cancellationToken = default)
        {
            _context.Customers.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Expression<Func<CustomerEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Customers.AnyAsync(predicate, cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
