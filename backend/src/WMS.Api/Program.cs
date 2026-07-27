using Serilog;
using WMS.Api.Middleware;
using WMS.BuildingBlocks.Application;
using WMS.BuildingBlocks.Infrastructure;
using WMS.Modules.Catalog.Infrastructure;
using WMS.Modules.Identity.Infrastructure;
using WMS.Modules.Inbound.Infrastructure;
using WMS.Modules.Inventory.Infrastructure;
using WMS.Modules.Outbound.Infrastructure;
using WMS.Modules.StockCount.Infrastructure;
using WMS.Modules.Transfer.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Cross-module building blocks: MediatR pipeline behaviors + EF Core domain-event dispatch.
    builder.Services.AddApplicationBehaviors();
    builder.Services.AddDomainEventDispatching();

    // Module composition roots. Each AddXxxModule() wires up that module's own DbContext,
    // repositories, and MediatR/FluentValidation registrations (no-op until the module has code).
    builder.Services
        .AddIdentityModule(builder.Configuration)
        .AddCatalogModule(builder.Configuration)
        .AddInventoryModule(builder.Configuration)
        .AddInboundModule(builder.Configuration)
        .AddOutboundModule(builder.Configuration)
        .AddTransferModule(builder.Configuration)
        .AddStockCountModule(builder.Configuration);

    var app = builder.Build();

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "WMS.Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
