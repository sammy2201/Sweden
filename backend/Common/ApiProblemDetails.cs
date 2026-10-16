using Microsoft.AspNetCore.Mvc;

namespace SwedenStart;

public static class ApiProblemDetails
{
     public static ProblemDetails Create(
         int status,
         string title,
         string detail,
         string? code = null)
     {
          var problem = new ProblemDetails
          {
               Status = status,
               Title = title,
               Detail = detail
          };

          if (!string.IsNullOrWhiteSpace(code))
          {
               problem.Extensions["code"] = code;
          }

          return problem;
     }
}