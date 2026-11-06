using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CustomerSync.Infrastructure.Persistence;

/// <summary>Used only by "dotnet ef" to build the model. It never opens a connection.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CustomerSyncDbContext>
{
    public CustomerSyncDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CustomerSyncDbContext>()
            .UseSqlServer("Server=localhost;Database=CustomerSync;User Id=sa;Password=unused-at-design-time;TrustServerCertificate=True")
            .Options;
        return new CustomerSyncDbContext(options);
    }
}