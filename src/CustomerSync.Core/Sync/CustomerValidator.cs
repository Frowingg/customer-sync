using System.Text.RegularExpressions;
using CustomerSync.Core.Models;

namespace CustomerSync.Core.Sync;

public static class CustomerValidator
{
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PersonTaxCode = new(@"^[A-Za-z0-9]{16}$", RegexOptions.Compiled);
    private static readonly Regex CompanyVat = new(@"^\d{11}$", RegexOptions.Compiled);

    public static IReadOnlyList<string> Validate(SourceCustomer customer)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(customer.Name))
            errors.Add("Name is required.");

        if (string.IsNullOrWhiteSpace(customer.Email) || !EmailPattern.IsMatch(customer.Email))
            errors.Add("Email is missing or malformed.");

        if (string.IsNullOrWhiteSpace(customer.TaxCode))
            errors.Add("Tax code is required.");
        else if (!PersonTaxCode.IsMatch(customer.TaxCode) && !CompanyVat.IsMatch(customer.TaxCode))
            errors.Add("Tax code must be 16 alphanumeric characters or 11 digits.");

        return errors;
    }
}