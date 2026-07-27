using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Identity.Application;
using WMS.Modules.Identity.Application.Abstractions;
using WMS.Modules.Identity.Infrastructure.Auth;
using WMS.Modules.Identity.Infrastructure.Persistence;
using WMS.Modules.Identity.Infrastructure.Repositories;
using WMS.Modules.Identity.Infrastructure.Seeding;

namespace WMS.Modules.Identity.Infrastructure;

/// <summary>
/// Composition root entry point for the Identity module. MediatR/FluentValidation are scoped to this
/// module's own Application assembly.
/// </summary>
public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(IdentityApplicationAssemblyMarker).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", IdentityDbContext.Schema));
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        services.AddScoped<IUserWriteRepository, UserWriteRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));

        return services;
    }
}
