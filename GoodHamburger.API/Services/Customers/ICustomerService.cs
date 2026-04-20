using GoodHamburger.Shared.DTOs.Customers;
using GoodHamburger.Shared.Models.Customers;

namespace GoodHamburger.API.Services.Customers;

public interface ICustomerService
{
    Task<Customer?> GetAsync(GetCustomerDto dto);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<IEnumerable<Customer>> FindAsync(FindCustomerDto dto);

    Task<Customer> CreateAsync(CreateCustomerDto dto);
    Task<Customer> UpdateAsync(UpdateCustomerDto dto);
    Task DeleteAsync(UpdateCustomerDto dto);
}
