using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SwedenStart;

[ApiController]
[Route("api/explore")]
[Authorize]
public class ExploreController : ControllerBase
{
     private readonly IExploreService _exploreService;
     private readonly string? _publicBaseUrl;

     public ExploreController(IExploreService exploreService, IConfiguration configuration)
     {
          _exploreService = exploreService;
          _publicBaseUrl = configuration["Api:BaseUrl"];
     }

     [HttpGet("counties")]
     [AllowAnonymous]
     public ActionResult<ExploreCountiesResponseDto> GetCounties()
     {
          var counties = _exploreService.GetCounties();

          return Ok(new ExploreCountiesResponseDto
          {
               Counties = counties.Select(c => new CountyDto
               {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = BuildAbsoluteImageUrl(c.ImageUrl)
               }).ToList()
          });
     }

     private string BuildAbsoluteImageUrl(string imageUrl)
     {
          if (string.IsNullOrWhiteSpace(imageUrl))
          {
               return imageUrl;
          }

          if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var parsedUri)
               && (parsedUri.Scheme == Uri.UriSchemeHttp || parsedUri.Scheme == Uri.UriSchemeHttps))
          {
               return imageUrl;
          }

          var baseUri = ResolveBaseUri();
          var normalizedPath = imageUrl.StartsWith('/') ? imageUrl : $"/{imageUrl}";

          return new Uri(baseUri, normalizedPath).ToString();
     }

     private Uri ResolveBaseUri()
     {
          if (!string.IsNullOrWhiteSpace(_publicBaseUrl)
              && Uri.TryCreate(_publicBaseUrl, UriKind.Absolute, out var configuredBaseUri))
          {
               return configuredBaseUri;
          }

          return new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}/");
     }

     [HttpGet("attractions")]
     [AllowAnonymous]
     public async Task<ActionResult<ExploreAttractionsResponseDto>> GetAttractions(
         [FromQuery] ExploreAttractionsRequestDto request,
         CancellationToken cancellationToken)
     {
          var result = await _exploreService.GetAttractionsAsync(
              request.County,
              request.Page,
              request.PageSize,
              request.Category);

          if (result.TotalCount == 0 && !await _exploreService.HasAnyAttractionsAsync(cancellationToken))
          {
               return Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Attraction catalogue unavailable",
                    detail: "Attraction data has not been synchronized yet. Please retry shortly.");
          }

          return Ok(new ExploreAttractionsResponseDto
          {
               Attractions = result.Items,
               Page = result.Page,
               PageSize = result.PageSize,
               TotalCount = result.TotalCount,
               TotalPages = result.TotalPages,
               HasNextPage = result.HasNextPage,
               HasPreviousPage = result.HasPreviousPage
          });
     }

     [HttpGet("attractions/{id}")]
     [AllowAnonymous]
     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
     public async Task<ActionResult<ExploreAttractionDetailResponseDto>> GetAttraction(string id)
     {
          var attraction = await _exploreService.GetAttractionAsync(id);

          if (attraction == null)
          {
               return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Attraction not found",
                    detail: "The requested attraction was not found.");
          }

          return Ok(new ExploreAttractionDetailResponseDto
          {
               Attraction = attraction
          });
     }
}
