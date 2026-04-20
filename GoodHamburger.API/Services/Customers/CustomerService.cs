using GoodHamburger.Shared.DTOs.Customers;
using GoodHamburger.Shared.Models.Customers;

namespace GoodHamburger.API.Services.Customers;

//TODO: Implementar o serviço de clientes
public class CustomerService : ICustomerService
{
    public Task<Customer> CreateAsync(CreateCustomerDto dto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(UpdateCustomerDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Customer>> FindAsync(FindCustomerDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Customer>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Customer?> GetAsync(GetCustomerDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<Customer> UpdateAsync(UpdateCustomerDto dto)
    {
        throw new NotImplementedException();
    }
}
