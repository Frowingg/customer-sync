namespace CustomerSync.Core.Models;

/// <summary>A customer record as returned by the legacy system.</summary>
public record SourceCustomer(int LegacyId, string? Name, string? Email, string? TaxCode, string? City);