using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.Modules.Catalog.Application;

namespace WMS.Modules.Catalog.Infrastructure;

/// <summary>
/// Composition root entry point for the Catalog module. MediatR/FluentValidation are scoped to this
/// module's own Application assembly. The DbContext, EF Core write repositories, and Dapper read
/// repositories are added here in Phase 3.
/// </summary>
public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(CatalogApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
