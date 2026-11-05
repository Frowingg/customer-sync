using CoreWCF;

namespace LegacyCustomers.Contracts;

[ServiceContract(Namespace = "http://legacy.example/customers")]
public interface ICustomerService
{
    [OperationContract]
    List<CustomerDto> GetCustomers();

    [OperationContract]
    CustomerDto? GetCustomer(int legacyId);
}