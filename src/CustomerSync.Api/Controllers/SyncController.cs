using CustomerSync.Api.Models;
using CustomerSync.Core.Models;
using CustomerSync.Core.Sync;
using CustomerSync.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerSync.Api.Controllers;

[ApiController]
[Route("api/sync/runs")]
public class SyncController : ControllerBase
{
    private readonly SyncService _sync;
    private readonly CustomerSyncDbContext _db;

    public SyncController(SyncService sync, CustomerSyncDbContext db)
    {
        _sync = sync;
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<SyncRunSummary>> Run(CancellationToken ct)
    {
        var run = await _sync.RunAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = run.Id }, SyncRunSummary.From(run));
    }

    [HttpGet]
    public async Task<ActionResult<List<SyncRunSummary>>> List(CancellationToken ct) =>
        await _db.SyncRuns.AsNoTracking()
            .OrderByDescending(r => r.StartedUtc)
            .Select(r => new SyncRunSummary(r.Id, r.StartedUtc, r.CompletedUtc,
                r.Inserted, r.Updated, r.Unchanged, r.Discrepancies.Count))
            .ToListAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SyncRunSummary>> Get(int id, CancellationToken ct)
    {
        var summary = await _db.SyncRuns.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new SyncRunSummary(r.Id, r.StartedUtc, r.CompletedUtc,
                r.Inserted, r.Updated, r.Unchanged, r.Discrepancies.Count))
            .FirstOrDefaultAsync(ct);

        if (summary is null) return NotFound();
        return summary;
    }

    [HttpGet("{id:int}/discrepancies")]
    public async Task<ActionResult<List<DiscrepancyDto>>> Discrepancies(
        int id, [FromQuery] DiscrepancyType? type, CancellationToken ct)
    {
        if (!await _db.SyncRuns.AnyAsync(r => r.Id == id, ct)) return NotFound();

        var query = _db.Discrepancies.AsNoTracking().Where(d => d.SyncRunId == id);
        if (type is not null) query = query.Where(d => d.Type == type.Value);

        return await query
            .OrderBy(d => d.LegacyId)
            .Select(d => new DiscrepancyDto(d.Id, d.Type, d.LegacyId, d.Detail))
            .ToListAsync(ct);
    }
}