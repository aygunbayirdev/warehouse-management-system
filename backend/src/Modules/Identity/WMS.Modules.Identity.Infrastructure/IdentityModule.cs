using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.Modules.Identity.Application;

namespace WMS.Modules.Identity.Infrastructure;

/// <summary>
/// Composition root entry point for the Identity module. MediatR/FluentValidation are scoped to this
/// module's own Application assembly. The DbContext, EF Core write repositories, and Dapper read
/// repositories are added here in Phase 2.
/// </summary>
public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(IdentityApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
