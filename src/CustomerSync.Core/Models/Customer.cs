namespace CustomerSync.Core.Models;

/// <summary>A customer as stored in the target database.</summary>
public class Customer
{
    public int Id { get; set; }
    public int LegacyId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string TaxCode { get; set; } = "";
    public string? City { get; set; }
    public DateTime UpdatedUtc { get; set; }
}