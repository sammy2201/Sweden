using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SwedenStart;

[ApiController]
[Route("api/explore")]
[Authorize]
public class ExploreController : ControllerBase
{
     private readonly IExploreService _exploreService;

     public ExploreController(IExploreService exploreService)
     {
          _exploreService = exploreService;
     }

     [HttpGet("counties")]
     [AllowAnonymous]
     public ActionResult<ExploreCountiesResponseDto> GetCounties()
     {
          var counties = _exploreService.GetCounties();

          return Ok(new ExploreCountiesResponseDto
          {
               Counties = counties.Select(county => new CountyDto
               {
                    Id = county.Id,
                    Name = county.Name,
                    Description = county.Description,
                    TransportProviderName = county.TransportProviderName,
                    OfficialTicketUrl = county.OfficialTicketUrl,
                    JourneyPlannerUrl = county.JourneyPlannerUrl
                }).ToList()
          });
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
               throw new ApiException(
                    StatusCodes.Status503ServiceUnavailable,
                    "Attraction catalogue unavailable",
                    "Attraction data has not been synchronized yet. Please retry shortly.");
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
               throw new ApiException(
                    StatusCodes.Status404NotFound,
                    "Attraction not found",
                    "The requested attraction was not found.");
          }

          return Ok(new ExploreAttractionDetailResponseDto
          {
               Attraction = attraction
          });
     }
}
