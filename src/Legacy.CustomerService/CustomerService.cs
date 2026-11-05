using LegacyCustomers.Contracts;
using LegacyCustomers.Data;

namespace LegacyCustomers;

public class CustomerService : ICustomerService
{
    private static readonly List<CustomerDto> Customers = SeedData.Create();

    public List<CustomerDto> GetCustomers() => Customers;

    public CustomerDto? GetCustomer(int legacyId) =>
        Customers.FirstOrDefault(c => c.LegacyId == legacyId);
}