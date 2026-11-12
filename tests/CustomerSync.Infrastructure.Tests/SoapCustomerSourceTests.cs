using CustomerSync.Core.Models;
using CustomerSync.Infrastructure.Soap;

namespace CustomerSync.Infrastructure.Tests;

public class SoapCustomerSourceTests : IClassFixture<LegacyServiceFixture>
{
    private readonly LegacyServiceFixture _legacy;

    public SoapCustomerSourceTests(LegacyServiceFixture legacy) => _legacy = legacy;

    private Task<IReadOnlyList<SourceCustomer>> FetchAsync() =>
        new SoapCustomerSource(_legacy.EndpointUrl).GetCustomersAsync();

    [Fact]
    public async Task GetCustomers_ReturnsEveryRecordFromTheLegacyService()
    {
        var customers = await FetchAsync();

        Assert.Equal(31, customers.Count); // 30 seeded records + duplicate of LegacyId 4
    }

    [Fact]
    public async Task GetCustomers_MapsAllFields()
    {
        var customers = await FetchAsync();

        Assert.Equal(
            new SourceCustomer(3, "Customer 3", "customer3@example.test", "DEMO0003A01H501Z", "Napoli"),
            Assert.Single(customers, c => c.LegacyId == 3));
        Assert.Equal(
            new SourceCustomer(5, "Demo Srl 5", "customer5@example.test", "10000000005", "Roma"),
            Assert.Single(customers, c => c.LegacyId == 5));
    }

    [Fact]
    public async Task GetCustomers_PassesDirtyDataThroughUnchanged()
    {
        var customers = await FetchAsync();

        Assert.Equal("customer7@@example", customers.Single(c => c.LegacyId == 7).Email);
        Assert.Equal("", customers.Single(c => c.LegacyId == 12).Name);
        Assert.Equal("SHORT", customers.Single(c => c.LegacyId == 15).TaxCode);
        Assert.Null(customers.Single(c => c.LegacyId == 25).TaxCode);
        Assert.Equal(2, customers.Count(c => c.LegacyId == 4));
    }

    [Fact]
    public async Task GetCustomers_WhenServiceIsUnreachable_ThrowsCommunicationException()
    {
        var url = $"http://127.0.0.1:{LegacyServiceFixture.GetFreePort()}/CustomerService.svc";
        var source = new SoapCustomerSource(url);

        await Assert.ThrowsAnyAsync<System.ServiceModel.CommunicationException>(
            () => source.GetCustomersAsync());
    }
}