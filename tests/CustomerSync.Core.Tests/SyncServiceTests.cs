using CustomerSync.Core.Models;
using CustomerSync.Core.Sync;
using Xunit;

namespace CustomerSync.Core.Tests;

public class SyncServiceTests
{
    private readonly FakeSource _source = new();
    private readonly FakeStore _store = new();
    private readonly SyncService _service;

    public SyncServiceTests() => _service = new SyncService(_source, _store);

    private static SourceCustomer Valid(int id, string city = "Roma") =>
        new(id, $"Customer {id}", $"c{id}@example.test", $"DEMO{id:D4}A01H501Z", city);

    [Fact]
    public async Task FirstRun_InsertsAllValidCustomers()
    {
        _source.Items.AddRange(new[] { Valid(1), Valid(2), Valid(3) });

        var run = await _service.RunAsync();

        Assert.Equal(3, run.Inserted);
        Assert.Equal(0, run.Updated);
        Assert.Equal(3, _store.Customers.Count);
        Assert.Empty(run.Discrepancies);
    }

    [Fact]
    public async Task SecondRunWithSameData_ChangesNothing()
    {
        _source.Items.AddRange(new[] { Valid(1), Valid(2) });
        await _service.RunAsync();

        var second = await _service.RunAsync();

        Assert.Equal(0, second.Inserted);
        Assert.Equal(0, second.Updated);
        Assert.Equal(2, second.Unchanged);
        Assert.Equal(2, _store.Customers.Count);
    }

    [Fact]
    public async Task ChangedSourceRecord_IsUpdated()
    {
        _source.Items.Add(Valid(1, "Roma"));
        await _service.RunAsync();
        _source.Items[0] = Valid(1, "Milano");

        var run = await _service.RunAsync();

        Assert.Equal(1, run.Updated);
        Assert.Equal("Milano", _store.Customers.Single().City);
    }

    [Fact]
    public async Task InvalidRecord_IsSkippedAndReported()
    {
        _source.Items.Add(Valid(1));
        _source.Items.Add(Valid(2) with { Email = "bad@@mail" });

        var run = await _service.RunAsync();

        Assert.Equal(1, run.Inserted);
        Assert.Single(_store.Customers);
        var d = Assert.Single(run.Discrepancies);
        Assert.Equal(DiscrepancyType.InvalidRecord, d.Type);
        Assert.Equal(2, d.LegacyId);
    }

    [Fact]
    public async Task DuplicateInSource_KeepsFirstAndReportsRest()
    {
        _source.Items.Add(Valid(4, "Roma"));
        _source.Items.Add(Valid(4, "Torino"));

        var run = await _service.RunAsync();

        Assert.Equal(1, run.Inserted);
        Assert.Equal("Roma", _store.Customers.Single().City);
        Assert.Equal(DiscrepancyType.DuplicateInSource, Assert.Single(run.Discrepancies).Type);
    }

    [Fact]
    public async Task CustomerMissingFromSource_IsReportedButNeverDeleted()
    {
        _source.Items.AddRange(new[] { Valid(1), Valid(2) });
        await _service.RunAsync();
        _source.Items.RemoveAt(1);

        var run = await _service.RunAsync();

        Assert.Equal(2, _store.Customers.Count);
        var d = Assert.Single(run.Discrepancies);
        Assert.Equal(DiscrepancyType.OrphanedInTarget, d.Type);
        Assert.Equal(2, d.LegacyId);
    }
}