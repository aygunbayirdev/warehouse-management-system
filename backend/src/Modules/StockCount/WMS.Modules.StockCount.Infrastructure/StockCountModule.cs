using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.StockCount.Application;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Infrastructure.Persistence;
using WMS.Modules.StockCount.Infrastructure.Repositories;

namespace WMS.Modules.StockCount.Infrastructure;

/// <summary>
/// Composition root entry point for the StockCount (stock counting + adjustment) module.
/// MediatR/FluentValidation are scoped to this module's own Application assembly.
/// </summary>
public static class StockCountModule
{
    public static IServiceCollection AddStockCountModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(StockCountApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<StockCountDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", StockCountDbContext.Schema));
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        services.AddScoped<IStockCountWriteRepository, StockCountWriteRepository>();
        services.AddScoped<IStockCountReadRepository, StockCountReadRepository>();
        services.AddScoped<IStockCountAdjustmentWriteRepository, StockCountAdjustmentWriteRepository>();
        services.AddScoped<IStockCountAdjustmentReadRepository, StockCountAdjustmentReadRepository>();

        return services;
    }
}
