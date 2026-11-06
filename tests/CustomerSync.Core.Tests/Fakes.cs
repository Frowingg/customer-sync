using CustomerSync.Core.Abstractions;
using CustomerSync.Core.Models;

namespace CustomerSync.Core.Tests;

public class FakeSource : ICustomerSource
{
    public List<SourceCustomer> Items { get; } = new();

    public Task<IReadOnlyList<SourceCustomer>> GetCustomersAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<SourceCustomer>>(Items.ToList());
}

public class FakeStore : ICustomerStore
{
    public List<Customer> Customers { get; } = new();
    public List<SyncRun> Runs { get; } = new();

    public Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Customer>>(Customers.ToList());

    public Task SaveAsync(SyncRun run, IReadOnlyList<Customer> newCustomers, CancellationToken ct = default)
    {
        Customers.AddRange(newCustomers);
        Runs.Add(run);
        return Task.CompletedTask;
    }
}