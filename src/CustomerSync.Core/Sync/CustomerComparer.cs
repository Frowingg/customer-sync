using CustomerSync.Core.Models;

namespace CustomerSync.Core.Sync;

public static class CustomerComparer
{
    public static IReadOnlyList<string> ChangedFields(Customer target, SourceCustomer source)
    {
        var changed = new List<string>();
        if (!string.Equals(target.Name, source.Name, StringComparison.Ordinal)) changed.Add(nameof(Customer.Name));
        if (!string.Equals(target.Email, source.Email, StringComparison.Ordinal)) changed.Add(nameof(Customer.Email));
        if (!string.Equals(target.TaxCode, source.TaxCode, StringComparison.Ordinal)) changed.Add(nameof(Customer.TaxCode));
        if (!string.Equals(target.City, source.City, StringComparison.Ordinal)) changed.Add(nameof(Customer.City));
        return changed;
    }

    public static void Apply(Customer target, SourceCustomer source, DateTime nowUtc)
    {
        target.Name = source.Name!;
        target.Email = source.Email!;
        target.TaxCode = source.TaxCode!;
        target.City = source.City;
        target.UpdatedUtc = nowUtc;
    }

    public static Customer ToCustomer(SourceCustomer source, DateTime nowUtc) => new()
    {
        LegacyId = source.LegacyId,
        Name = source.Name!,
        Email = source.Email!,
        TaxCode = source.TaxCode!,
        City = source.City,
        UpdatedUtc = nowUtc
    };
}