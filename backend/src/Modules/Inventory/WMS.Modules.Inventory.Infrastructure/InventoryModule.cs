using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Inventory.Application;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Infrastructure.Persistence;
using WMS.Modules.Inventory.Infrastructure.Repositories;

namespace WMS.Modules.Inventory.Infrastructure;

/// <summary>
/// Composition root entry point for the Inventory module. MediatR/FluentValidation are scoped to this
/// module's own Application assembly.
/// </summary>
public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(InventoryApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<InventoryDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", InventoryDbContext.Schema));
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        services.AddScoped<IWarehouseWriteRepository, WarehouseWriteRepository>();
        services.AddScoped<IWarehouseReadRepository, WarehouseReadRepository>();
        services.AddScoped<IStockItemWriteRepository, StockItemWriteRepository>();
        services.AddScoped<IStockMovementWriteRepository, StockMovementWriteRepository>();
        services.AddScoped<IStockItemReadRepository, StockItemReadRepository>();

        return services;
    }
}
