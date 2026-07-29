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
     public async Task<ActionResult<ExploreCountiesResponseDto>> GetCounties()
     {
          var counties = await _exploreService.GetCountiesAsync();

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
     public async Task<ActionResult<ExploreAttractionsResponseDto>> GetAttractions(
         [FromQuery] ExploreAttractionsRequestDto request)
     {
          var result = await _exploreService.GetAttractionsAsync(
              request.County,
              request.Page,
              request.PageSize,
              request.Category);

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
     public async Task<ActionResult<ExploreAttractionDetailResponseDto>> GetAttraction(string id)
     {
          var attraction = await _exploreService.GetAttractionAsync(id);

          if (attraction == null)
          {
               return NotFound();
          }

          return Ok(new ExploreAttractionDetailResponseDto
          {
               Attraction = attraction
          });
     }
}