using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using WMS.SharedKernel;

namespace WMS.BuildingBlocks.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Handling {RequestName}", requestName);

        var response = await next(cancellationToken);

        stopwatch.Stop();

        if (response.IsSuccess)
        {
            logger.LogInformation(
                "Handled {RequestName} successfully in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
        }
        else
        {
            logger.LogWarning(
                "Handled {RequestName} with error {ErrorCode}: {ErrorMessage} in {ElapsedMilliseconds}ms",
                requestName,
                response.Error.Code,
                response.Error.Message,
                stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
