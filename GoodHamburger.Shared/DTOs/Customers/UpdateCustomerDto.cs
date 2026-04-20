namespace GoodHamburger.Shared.DTOs.Customers;

public sealed record UpdateCustomerDto(Guid Id, string Name, string Phone, string Address);
