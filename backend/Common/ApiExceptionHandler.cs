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
        CancellationToken cancellationToken)
    {
        var (status, title, detail, code) = exception switch
        {
            ApiException ex => (ex.StatusCode, ex.Title, ex.Message, ex.Code),
            UnauthorizedAccessException ex => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                ex.Message,
                (string?)null),
            ArgumentException ex => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                ex.Message,
                (string?)null),
            KeyNotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                ex.Message,
                (string?)null),
            HttpRequestException => (
                StatusCodes.Status502BadGateway,
                "External service unavailable",
                "A required external service is unavailable.",
                (string?)null),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Server error",
                "An unexpected error occurred.",
                (string?)null)
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled API exception for {Path}", httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation(exception, "API request failed with status {StatusCode}", status);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        if (!string.IsNullOrWhiteSpace(code))
        {
            problem.Extensions["code"] = code;
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
