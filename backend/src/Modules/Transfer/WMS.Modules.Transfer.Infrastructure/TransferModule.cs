using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Transfer.Application;
using WMS.Modules.Transfer.Application.Abstractions;
using WMS.Modules.Transfer.Infrastructure.Persistence;
using WMS.Modules.Transfer.Infrastructure.Repositories;

namespace WMS.Modules.Transfer.Infrastructure;

/// <summary>
/// Composition root entry point for the Transfer (inter-warehouse) module. MediatR/FluentValidation
/// are scoped to this module's own Application assembly.
/// </summary>
public static class TransferModule
{
    public static IServiceCollection AddTransferModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(TransferApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<TransferDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", TransferDbContext.Schema));
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        services.AddScoped<IStockTransferWriteRepository, StockTransferWriteRepository>();
        services.AddScoped<IStockTransferReadRepository, StockTransferReadRepository>();

        return services;
    }
}
