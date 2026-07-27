using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Application.Behaviors;

namespace WMS.BuildingBlocks.Application;

/// <summary>
/// Registers the cross-module MediatR pipeline behaviors. Call once from the composition root (WMS.Api).
/// Each module registers its own handlers/validators against its own Application assembly separately.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationBehaviors(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
