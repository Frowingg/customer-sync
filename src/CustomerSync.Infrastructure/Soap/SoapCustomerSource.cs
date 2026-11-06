using System.ServiceModel;
using CustomerSync.Core.Abstractions;
using CustomerSync.Core.Models;

namespace CustomerSync.Infrastructure.Soap;

public class SoapCustomerSource : ICustomerSource
{
    private readonly string _endpointUrl;

    public SoapCustomerSource(string endpointUrl) => _endpointUrl = endpointUrl;

    public async Task<IReadOnlyList<SourceCustomer>> GetCustomersAsync(CancellationToken ct = default)
    {
        var binding = new BasicHttpBinding
        {
            MaxReceivedMessageSize = 10 * 1024 * 1024,
            SendTimeout = TimeSpan.FromSeconds(30),
            ReceiveTimeout = TimeSpan.FromSeconds(30)
        };

        var factory = new ChannelFactory<ILegacyCustomerService>(binding, new EndpointAddress(_endpointUrl));
        var channel = factory.CreateChannel();

        try
        {
            var customers = await channel.GetCustomersAsync();
            ((IClientChannel)channel).Close();
            factory.Close();

            return customers
                .Select(c => new SourceCustomer(c.LegacyId, c.Name, c.Email, c.TaxCode, c.City))
                .ToList();
        }
        catch
        {
            ((IClientChannel)channel).Abort();
            factory.Abort();
            throw;
        }
    }
}