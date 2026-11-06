using CustomerSync.Core.Abstractions;
using CustomerSync.Infrastructure.Persistence;
using CustomerSync.Infrastructure.Soap;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerSync.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, string legacyServiceUrl)
    {
        services.AddSingleton<IDatabaseConfigurator>(new SqlServerConfigurator(connectionString));
        services.AddDbContext<CustomerSyncDbContext>((sp, options) =>
            sp.GetRequiredService<IDatabaseConfigurator>().Configure(options));        services.AddScoped<ICustomerStore, SqlCustomerStore>();
        services.AddScoped<ICustomerSource>(_ => new SoapCustomerSource(legacyServiceUrl));
        return services;
    }
}