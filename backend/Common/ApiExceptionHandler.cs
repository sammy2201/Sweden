using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SwedenStart;

public sealed class ApiExceptionHandler(
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
               RoadmapConflictException ex => (
                   StatusCodes.Status409Conflict,
                   "Roadmap conflict",
                   ex.Message,
                   "ROADMAP_ALREADY_EXISTS"),
               RoadmapNotFoundException ex => (
                   StatusCodes.Status404NotFound,
                   "Roadmap not found",
                   ex.Message,
                   "ROADMAP_NOT_FOUND"),
               RoadmapConcurrencyException ex => (
                   StatusCodes.Status409Conflict,
                   "Roadmap update conflict",
                   ex.Message,
                   "ROADMAP_CONCURRENCY_CONFLICT"),
               RoadmapPrerequisiteException ex => (
                   StatusCodes.Status409Conflict,
                   "Step prerequisites are incomplete",
                   ex.Message,
                   "ROADMAP_PREREQUISITES_INCOMPLETE"),
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

          var problem = ApiProblemDetails.Create(status, title, detail, code);
          problem.Instance = httpContext.Request.Path;
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
