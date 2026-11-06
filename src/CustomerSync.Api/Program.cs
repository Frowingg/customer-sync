using System.Text.Json.Serialization;
using CustomerSync.Core.Sync;
using CustomerSync.Infrastructure;
using CustomerSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");
var legacyUrl = builder.Configuration["LegacyService:Url"]
    ?? throw new InvalidOperationException("LegacyService:Url is not configured.");

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(connectionString, legacyUrl);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<SyncService>();
builder.Services.AddHealthChecks().AddDbContextCheck<CustomerSyncDbContext>();

var app = builder.Build();

// Demo convenience: create/update the schema at startup.
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<CustomerSyncDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }