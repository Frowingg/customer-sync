using System.ServiceModel;

namespace CustomerSync.Infrastructure.Soap;

[ServiceContract(Namespace = "http://legacy.example/customers", Name = "ICustomerService")]
public interface ILegacyCustomerService
{
    [OperationContract(
        Action = "http://legacy.example/customers/ICustomerService/GetCustomers",
        ReplyAction = "http://legacy.example/customers/ICustomerService/GetCustomersResponse")]
    Task<List<CustomerDto>> GetCustomersAsync();
}