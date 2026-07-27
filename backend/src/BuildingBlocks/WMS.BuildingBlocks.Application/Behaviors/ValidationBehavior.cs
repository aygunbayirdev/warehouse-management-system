using System.Reflection;
using FluentValidation;
using MediatR;
using WMS.SharedKernel;

namespace WMS.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Runs all registered FluentValidation validators for the request before it reaches its handler.
/// Requires <typeparamref name="TResponse"/> to be <see cref="Result"/> or <see cref="Result{TValue}"/>
/// so validation failures can be returned as a failed Result instead of throwing.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        var error = Error.Validation(
            "Validation.Failed",
            string.Join(" | ", failures.Select(f => f.ErrorMessage)));

        return CreateValidationFailureResult(error);
    }

    private static TResponse CreateValidationFailureResult(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var genericFailureMethod = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition);

        var failureMethod = genericFailureMethod.MakeGenericMethod(typeof(TResponse).GenericTypeArguments[0]);

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }
}
