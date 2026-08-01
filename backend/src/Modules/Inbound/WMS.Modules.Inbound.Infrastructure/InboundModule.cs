using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure.Outbox;
using WMS.Modules.Inbound.Application;
using WMS.Modules.Inbound.Application.Abstractions;
using WMS.Modules.Inbound.Infrastructure.Persistence;
using WMS.Modules.Inbound.Infrastructure.Repositories;

namespace WMS.Modules.Inbound.Infrastructure;

/// <summary>
/// Composition root entry point for the Inbound (goods receipt) module. MediatR/FluentValidation are
/// scoped to this module's own Application assembly.
/// </summary>
public static class InboundModule
{
    public static IServiceCollection AddInboundModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(InboundApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<InboundDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", InboundDbContext.Schema));
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(sp.GetRequiredService<OutboxWritingInterceptor>());
        });

        services.AddScoped<IGoodsReceiptWriteRepository, GoodsReceiptWriteRepository>();
        services.AddScoped<IGoodsReceiptReadRepository, GoodsReceiptReadRepository>();

        services.AddHostedService<OutboxProcessor<InboundDbContext>>();

        return services;
    }
}
