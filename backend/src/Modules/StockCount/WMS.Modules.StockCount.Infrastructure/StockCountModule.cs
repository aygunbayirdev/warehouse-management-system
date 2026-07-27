using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.Modules.StockCount.Application;

namespace WMS.Modules.StockCount.Infrastructure;

/// <summary>
/// Composition root entry point for the StockCount (stock counting + adjustment) module.
/// MediatR/FluentValidation are scoped to this module's own Application assembly. The DbContext,
/// EF Core write repositories, and Dapper read repositories are added here in Phase 8.
/// </summary>
public static class StockCountModule
{
    public static IServiceCollection AddStockCountModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(StockCountApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
