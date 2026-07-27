using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure.Persistence;

namespace WMS.BuildingBlocks.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="DomainEventDispatchInterceptor"/> so module DbContexts can pull it in via
    /// <c>options.AddInterceptors(sp.GetRequiredService&lt;DomainEventDispatchInterceptor&gt;())</c>.
    /// </summary>
    public static IServiceCollection AddDomainEventDispatching(this IServiceCollection services)
    {
        services.AddScoped<DomainEventDispatchInterceptor>();

        return services;
    }
}
