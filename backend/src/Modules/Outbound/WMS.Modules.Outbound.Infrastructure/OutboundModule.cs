using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.Modules.Outbound.Application;

namespace WMS.Modules.Outbound.Infrastructure;

/// <summary>
/// Composition root entry point for the Outbound (goods issue / shipment) module. MediatR/FluentValidation
/// are scoped to this module's own Application assembly. The DbContext, EF Core write repositories, and
/// Dapper read repositories are added here in Phase 6.
/// </summary>
public static class OutboundModule
{
    public static IServiceCollection AddOutboundModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(OutboundApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
