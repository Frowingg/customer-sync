namespace CustomerSync.Core.Models;

public class Discrepancy
{
    public int Id { get; set; }
    public int SyncRunId { get; set; }
    public DiscrepancyType Type { get; set; }
    public int LegacyId { get; set; }
    public string Detail { get; set; } = "";
}