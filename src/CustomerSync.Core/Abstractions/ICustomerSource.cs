using CustomerSync.Core.Models;

namespace CustomerSync.Core.Abstractions;

public interface ICustomerSource
{
    Task<IReadOnlyList<SourceCustomer>> GetCustomersAsync(CancellationToken ct = default);
}