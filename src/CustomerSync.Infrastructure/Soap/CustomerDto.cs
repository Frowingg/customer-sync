using System.Runtime.Serialization;

namespace CustomerSync.Infrastructure.Soap;

[DataContract(Namespace = "http://legacy.example/customers")]
public class CustomerDto
{
    [DataMember] public int LegacyId { get; set; }
    [DataMember] public string? Name { get; set; }
    [DataMember] public string? Email { get; set; }
    [DataMember] public string? TaxCode { get; set; }
    [DataMember] public string? City { get; set; }
}