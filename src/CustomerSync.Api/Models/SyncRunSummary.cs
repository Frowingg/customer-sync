using CustomerSync.Core.Models;

namespace CustomerSync.Api.Models;

public record SyncRunSummary(
    int Id, DateTime StartedUtc, DateTime? CompletedUtc,
    int Inserted, int Updated, int Unchanged, int DiscrepancyCount)
{
    public static SyncRunSummary From(SyncRun r) => new(
        r.Id, r.StartedUtc, r.CompletedUtc, r.Inserted, r.Updated, r.Unchanged, r.Discrepancies.Count);
}

public record DiscrepancyDto(int Id, DiscrepancyType Type, int LegacyId, string Detail);