using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure.Outbox;
using WMS.Modules.Outbound.Application;
using WMS.Modules.Outbound.Application.Abstractions;
using WMS.Modules.Outbound.Infrastructure.Persistence;
using WMS.Modules.Outbound.Infrastructure.Repositories;

namespace WMS.Modules.Outbound.Infrastructure;

/// <summary>
/// Composition root entry point for the Outbound (goods issue / shipment) module. MediatR/FluentValidation
/// are scoped to this module's own Application assembly.
/// </summary>
public static class OutboundModule
{
    public static IServiceCollection AddOutboundModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(OutboundApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<OutboundDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", OutboundDbContext.Schema));
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(sp.GetRequiredService<OutboxWritingInterceptor>());
        });

        services.AddScoped<IGoodsIssueWriteRepository, GoodsIssueWriteRepository>();
        services.AddScoped<IGoodsIssueReadRepository, GoodsIssueReadRepository>();

        services.AddHostedService<OutboxProcessor<OutboundDbContext>>();

        return services;
    }
}
