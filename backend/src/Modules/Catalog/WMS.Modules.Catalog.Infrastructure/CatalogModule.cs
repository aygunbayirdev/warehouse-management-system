using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Catalog.Application;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Infrastructure.Persistence;
using WMS.Modules.Catalog.Infrastructure.Repositories;

namespace WMS.Modules.Catalog.Infrastructure;

/// <summary>
/// Composition root entry point for the Catalog module. MediatR/FluentValidation are scoped to this
/// module's own Application assembly.
/// </summary>
public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(CatalogApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<CatalogDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", CatalogDbContext.Schema));
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        services.AddScoped<IUnitOfMeasureWriteRepository, UnitOfMeasureWriteRepository>();
        services.AddScoped<IUnitOfMeasureReadRepository, UnitOfMeasureReadRepository>();
        services.AddScoped<ICategoryWriteRepository, CategoryWriteRepository>();
        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();
        services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();

        return services;
    }
}
