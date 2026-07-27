using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.Modules.Inbound.Application;

namespace WMS.Modules.Inbound.Infrastructure;

/// <summary>
/// Composition root entry point for the Inbound (goods receipt) module. MediatR/FluentValidation are
/// scoped to this module's own Application assembly. The DbContext, EF Core write repositories, and
/// Dapper read repositories are added here in Phase 5.
/// </summary>
public static class InboundModule
{
    public static IServiceCollection AddInboundModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(InboundApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
