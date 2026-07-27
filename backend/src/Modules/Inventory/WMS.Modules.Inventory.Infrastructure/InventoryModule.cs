using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.Modules.Inventory.Application;

namespace WMS.Modules.Inventory.Infrastructure;

/// <summary>
/// Composition root entry point for the Inventory module. MediatR/FluentValidation are scoped to this
/// module's own Application assembly. The DbContext, EF Core write repositories, and Dapper read
/// repositories are added here in Phase 4.
/// </summary>
public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(InventoryApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
