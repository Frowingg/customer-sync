using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using LegacyCustomers;
using LegacyCustomers.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AllowSynchronousIO = true);
builder.Services.AddServiceModelServices().AddServiceModelMetadata();

var app = builder.Build();

app.Services.GetRequiredService<ServiceMetadataBehavior>().HttpGetEnabled = true;

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CustomerService>()
        .AddServiceEndpoint<CustomerService, ICustomerService>(new BasicHttpBinding(), "/CustomerService.svc");
});

app.Run();