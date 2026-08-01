using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure.Outbox;
using WMS.BuildingBlocks.Infrastructure.Persistence;

namespace WMS.BuildingBlocks.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="OutboxWritingInterceptor"/> so module DbContexts can pull it in via
    /// <c>options.AddInterceptors(sp.GetRequiredService&lt;OutboxWritingInterceptor&gt;())</c>.
    /// </summary>
    public static IServiceCollection AddDomainEventOutbox(this IServiceCollection services)
    {
        services.AddScoped<OutboxWritingInterceptor>();

        return services;
    }

    /// <summary>Registers the single <see cref="ISqlConnectionFactory"/> shared by every module's Dapper read repositories.</summary>
    public static IServiceCollection AddSqlConnectionFactory(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<ISqlConnectionFactory>(new NpgsqlConnectionFactory(connectionString));

        return services;
    }
}
