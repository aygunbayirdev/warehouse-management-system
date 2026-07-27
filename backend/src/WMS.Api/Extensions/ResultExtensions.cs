using Microsoft.AspNetCore.Mvc;
using WMS.SharedKernel;

namespace WMS.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<TValue>(this Result<TValue> result) =>
        result.IsSuccess ? new OkObjectResult(result.Value) : CreateProblemResult(result.Error);

    public static IActionResult ToActionResult(this Result result) =>
        result.IsSuccess ? new NoContentResult() : CreateProblemResult(result.Error);

    private static IActionResult CreateProblemResult(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest,
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Message,
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
