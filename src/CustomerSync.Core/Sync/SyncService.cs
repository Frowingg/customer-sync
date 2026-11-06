using CustomerSync.Core.Abstractions;
using CustomerSync.Core.Models;

namespace CustomerSync.Core.Sync;

public class SyncService
{
    private readonly ICustomerSource _source;
    private readonly ICustomerStore _store;
    private readonly TimeProvider _time;

    public SyncService(ICustomerSource source, ICustomerStore store, TimeProvider? time = null)
    {
        _source = source;
        _store = store;
        _time = time ?? TimeProvider.System;
    }

    public async Task<SyncRun> RunAsync(CancellationToken ct = default)
    {
        var now = _time.GetUtcNow().UtcDateTime;
        var run = new SyncRun { StartedUtc = now };

        var source = await _source.GetCustomersAsync(ct);
        var existing = await _store.GetAllAsync(ct);
        var byLegacyId = existing.ToDictionary(c => c.LegacyId);

        var seen = new HashSet<int>();
        var toInsert = new List<Customer>();

        void Report(DiscrepancyType type, int legacyId, string detail) =>
            run.Discrepancies.Add(new Discrepancy { Type = type, LegacyId = legacyId, Detail = detail });

        foreach (var record in source)
        {
            if (!seen.Add(record.LegacyId))
            {
                Report(DiscrepancyType.DuplicateInSource, record.LegacyId,
                    "LegacyId returned more than once; the first occurrence was kept.");
                continue;
            }

            var errors = CustomerValidator.Validate(record);
            if (errors.Count > 0)
            {
                Report(DiscrepancyType.InvalidRecord, record.LegacyId, string.Join(" ", errors));
                continue;
            }

            if (byLegacyId.TryGetValue(record.LegacyId, out var current))
            {
                if (CustomerComparer.ChangedFields(current, record).Count == 0)
                {
                    run.Unchanged++;
                }
                else
                {
                    CustomerComparer.Apply(current, record, now);
                    run.Updated++;
                }
            }
            else
            {
                toInsert.Add(CustomerComparer.ToCustomer(record, now));
                run.Inserted++;
            }
        }

        foreach (var orphan in existing.Where(c => !seen.Contains(c.LegacyId)))
        {
            Report(DiscrepancyType.OrphanedInTarget, orphan.LegacyId,
                "Present in the target but no longer returned by the source. Not deleted.");
        }

        run.CompletedUtc = _time.GetUtcNow().UtcDateTime;
        await _store.SaveAsync(run, toInsert, ct);
        return run;
    }
}