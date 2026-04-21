using GoodHamburger.API.Entities.Customers;
using GoodHamburger.API.Extensions.Entities;
using GoodHamburger.API.Repositories.Customers;
using GoodHamburger.Shared.DTOs.Customers;
using GoodHamburger.Shared.Models.Customers;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.API.Services.Customers;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    public async Task<IEnumerable<Customer>> FindAsync(FindCustomerDto dto)
    {
        var entites = await _customerRepository.FindAsync(e => dto.Name != null && EF.Functions.Like(e.Name, $"%{dto.Name}") || dto.Phone != null && e.Phone == dto.Phone);
        return entites.Select(e => e.MapEntityToModel());
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return (await _customerRepository.GetAllAsync()).Select(e => e.MapEntityToModel());
    }

    public async Task<Customer?> GetAsync(GetCustomerDto dto)
    {
        var customerEntity = await _customerRepository.GetByIdAsync(dto.Id);
        if (customerEntity is null)
            return null;

        return customerEntity.MapEntityToModel();
    }

    public async Task<Customer> CreateAsync(CreateCustomerDto dto)
    {
        var customerEntity = await _customerRepository.AddAsync(new CustomerEntity
        {
            Name = dto.Name.Trim(),
            Phone = dto.Phone.Trim(),
            Address = dto.Address.Trim(),
        });
        await _customerRepository.SaveChangesAsync();
        return customerEntity.MapEntityToModel();
    }

    public async Task<Customer> UpdateAsync(UpdateCustomerDto dto)
    {
        await _customerRepository.UpdateAsync(new CustomerEntity
        {
            Id = dto.Id,
            Name = dto.Name.Trim(),
            Phone = dto.Phone.Trim(),
            Address = dto.Address.Trim(),
        });
        await _customerRepository.SaveChangesAsync();

        return await GetAsync(new GetCustomerDto(dto.Id)) ?? throw new InvalidOperationException("Failed to retrieve the updated customer.");
    }

    public async Task DeleteAsync(UpdateCustomerDto dto)
    {
        var customerEntity = await _customerRepository.GetByIdAsync(dto.Id) ?? throw new InvalidOperationException("Customer not found.");
        if(customerEntity.Orders.Count != 0)
            throw new InvalidOperationException("Cannot delete a customer with existing orders.");

        await _customerRepository.DeleteAsync(customerEntity);
        await _customerRepository.SaveChangesAsync();
    }
}
