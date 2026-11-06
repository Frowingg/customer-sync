using CustomerSync.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerSync.Infrastructure.Persistence;

public class CustomerSyncDbContext : DbContext
{
    public CustomerSyncDbContext(DbContextOptions<CustomerSyncDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SyncRun> SyncRuns => Set<SyncRun>();
    public DbSet<Discrepancy> Discrepancies => Set<Discrepancy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasIndex(c => c.LegacyId).IsUnique();
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Email).HasMaxLength(320).IsRequired();
            e.Property(c => c.TaxCode).HasMaxLength(16).IsRequired();
            e.Property(c => c.City).HasMaxLength(100);
        });

        modelBuilder.Entity<SyncRun>(e =>
        {
            e.HasMany(r => r.Discrepancies)
                .WithOne()
                .HasForeignKey(d => d.SyncRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Discrepancy>(e =>
        {
            e.Property(d => d.Type).HasConversion<string>().HasMaxLength(30);
            e.Property(d => d.Detail).HasMaxLength(1000);
            e.HasIndex(d => d.SyncRunId);
        });
    }
}