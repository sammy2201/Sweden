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
        var upstreamException = exception as HttpRequestException;
        var status = apiException?.StatusCode
            ?? (upstreamException is not null
                ? StatusCodes.Status502BadGateway
                : StatusCodes.Status500InternalServerError);

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled API exception");
        }
        else
        {
            logger.LogInformation("API request failed with status {StatusCode}", status);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = apiException?.Title
                ?? (upstreamException is not null ? "Upstream service unavailable" : "Server error"),
            Detail = apiException?.Message
                ?? (upstreamException is not null
                    ? "A required external service is unavailable."
                    : "An unexpected error occurred."),
            Instance = httpContext.Request.Path
        };

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
