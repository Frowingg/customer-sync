using System.Net;
using System.Net.Sockets;
using CoreWCF.Configuration;
using LegacyCustomers;
using LegacyCustomers.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace CustomerSync.Infrastructure.Tests;

/// <summary>Hosts the real legacy SOAP service (CoreWCF) on a free local port for the duration of the tests.</summary>
public sealed class LegacyServiceFixture : IAsyncLifetime
{
    private WebApplication? _app;

    public string EndpointUrl { get; private set; } = "";

    public async Task InitializeAsync()
    {
        var port = GetFreePort();

        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AllowSynchronousIO = true;
            options.Listen(IPAddress.Loopback, port);
        });
        builder.Services.AddServiceModelServices();

        _app = builder.Build();
        _app.UseServiceModel(serviceBuilder =>
        {
            serviceBuilder.AddService<CustomerService>()
                .AddServiceEndpoint<CustomerService, ICustomerService>(
                    new CoreWCF.BasicHttpBinding(), "/CustomerService.svc");
        });

        await _app.StartAsync();
        EndpointUrl = $"http://127.0.0.1:{port}/CustomerService.svc";
    }

    public async Task DisposeAsync()
    {
        if (_app is not null) await _app.DisposeAsync();
    }

    public static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}