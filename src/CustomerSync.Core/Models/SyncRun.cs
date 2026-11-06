namespace CustomerSync.Core.Models;

public class SyncRun
{
    public int Id { get; set; }
    public DateTime StartedUtc { get; set; }
    public DateTime? CompletedUtc { get; set; }
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Unchanged { get; set; }
    public List<Discrepancy> Discrepancies { get; set; } = new();
}