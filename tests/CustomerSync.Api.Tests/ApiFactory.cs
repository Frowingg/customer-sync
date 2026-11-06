using CustomerSync.Core.Abstractions;
using CustomerSync.Core.Models;
using CustomerSync.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CustomerSync.Api.Tests;

public class FakeSource : ICustomerSource
{
    public List<SourceCustomer> Items { get; } = new();

    public Task<IReadOnlyList<SourceCustomer>> GetCustomersAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<SourceCustomer>>(Items.ToList());
}

public class SqliteConfigurator : IDatabaseConfigurator
{
    private readonly SqliteConnection _connection;

    public SqliteConfigurator(SqliteConnection connection) => _connection = connection;

    public void Configure(DbContextOptionsBuilder options) => options.UseSqlite(_connection);
}

/// <summary>Runs the real API in memory, with SQLite instead of SQL Server and a fake legacy source.</summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public FakeSource Source { get; } = new();

    public ApiFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", "unused-in-tests");
        builder.UseSetting("LegacyService:Url", "http://unused-in-tests");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDatabaseConfigurator>();
            services.AddSingleton<IDatabaseConfigurator>(new SqliteConfigurator(_connection));

            services.RemoveAll<ICustomerSource>();
            services.AddSingleton<ICustomerSource>(Source);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<CustomerSyncDbContext>().Database.EnsureCreated();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}