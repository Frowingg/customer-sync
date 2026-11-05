using LegacyCustomers.Contracts;

namespace LegacyCustomers.Data;

/// <summary>Deterministic fake data with deliberate anomalies. Everything is invented.</summary>
public static class SeedData
{
    private static readonly string[] Cities = { "Roma", "Milano", "Torino", "Napoli", "Bologna" };

    public static List<CustomerDto> Create()
    {
        var list = new List<CustomerDto>();
        for (var id = 1; id <= 30; id++)
        {
            var isCompany = id % 5 == 0;
            list.Add(new CustomerDto
            {
                LegacyId = id,
                Name = isCompany ? $"Demo Srl {id}" : $"Customer {id}",
                Email = $"customer{id}@example.test",
                // Fake tax codes: 16 chars for people, 11 digits for companies.
                TaxCode = isCompany ? (10_000_000_000L + id).ToString() : $"DEMO{id:D4}A01H501Z",
                City = Cities[id % Cities.Length]
            });
        }

        // Deliberate anomalies
        list[6].Email = "customer7@@example";        // id 7: malformed email
        list[11].Name = "";                           // id 12: missing name
        list[14].TaxCode = "SHORT";                   // id 15: invalid tax code
        list[24].TaxCode = null;                      // id 25: missing tax code
        list.Add(new CustomerDto                      // id 4 returned twice
        {
            LegacyId = 4, Name = "Customer 4 (dup)", Email = "customer4@example.test",
            TaxCode = "DEMO0004A01H501Z", City = "Roma"
        });

        return list;
    }
}