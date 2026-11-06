using CustomerSync.Core.Abstractions;
using CustomerSync.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerSync.Infrastructure.Persistence;

public class SqlCustomerStore : ICustomerStore
{
    private readonly CustomerSyncDbContext _db;

    public SqlCustomerStore(CustomerSyncDbContext db) => _db = db;

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Customers.ToListAsync(ct);

    public async Task SaveAsync(SyncRun run, IReadOnlyList<Customer> newCustomers, CancellationToken ct = default)
    {
        _db.Customers.AddRange(newCustomers);
        _db.SyncRuns.Add(run);
        await _db.SaveChangesAsync(ct); // one transaction: inserts, updates and the run report together
    }
}