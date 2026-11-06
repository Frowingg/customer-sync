using Microsoft.EntityFrameworkCore;

namespace CustomerSync.Infrastructure.Persistence;

/// <summary>Chooses the database provider. Replaceable in tests.</summary>
public interface IDatabaseConfigurator
{
    void Configure(DbContextOptionsBuilder options);
}

public class SqlServerConfigurator : IDatabaseConfigurator
{
    private readonly string _connectionString;

    public SqlServerConfigurator(string connectionString) => _connectionString = connectionString;

    public void Configure(DbContextOptionsBuilder options) => options.UseSqlServer(_connectionString);
}