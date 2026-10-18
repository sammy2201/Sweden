using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SwedenStart;

public class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken _)
    {
        var apiException = exception as ApiException;
        var status = apiException?.StatusCode ?? StatusCodes.Status500InternalServerError;

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled API exception for {Path}", httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("API request failed with status {StatusCode} for {Path}", status, httpContext.Request.Path);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = apiException?.Title ?? "Server error",
            Detail = apiException?.Message ?? "An unexpected error occurred.",
            Instance = httpContext.Request.Path
        };
        if (!string.IsNullOrWhiteSpace(apiException?.Code))
        {
            problem.Extensions["code"] = apiException.Code;
        }
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        });

        return true;
    }
}
