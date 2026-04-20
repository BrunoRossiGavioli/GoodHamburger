using GoodHamburger.API.Entities.Customers;

namespace GoodHamburger.API.Repositories.Customers
{
    public interface ICustomerRepository : IRepository<CustomerEntity, Guid>
    {
    }
}
