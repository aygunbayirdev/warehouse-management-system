using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.Modules.Transfer.Application;

namespace WMS.Modules.Transfer.Infrastructure;

/// <summary>
/// Composition root entry point for the Transfer (inter-warehouse) module. MediatR/FluentValidation
/// are scoped to this module's own Application assembly. The DbContext, EF Core write repositories,
/// and Dapper read repositories are added here in Phase 7.
/// </summary>
public static class TransferModule
{
    public static IServiceCollection AddTransferModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(TransferApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
