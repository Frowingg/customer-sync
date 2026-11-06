using CustomerSync.Core.Models;

namespace CustomerSync.Core.Abstractions;

public interface ICustomerStore
{
    /// <summary>Returns tracked entities: changes made to them are persisted by SaveAsync.</summary>
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Persists the run, the new customers and any change made to tracked customers.</summary>
    Task SaveAsync(SyncRun run, IReadOnlyList<Customer> newCustomers, CancellationToken ct = default);
}