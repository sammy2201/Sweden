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
     public ActionResult<ExploreCountiesResponseDto> GetCountiesController()
     {
          var counties = CountyData.GetCounties();

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
     public async Task<ActionResult<ExploreAttractionsResponseDto>> GetAttractionsControllerAsync(
         [FromQuery] ExploreAttractionsRequestDto request)
     {
          var result = await _exploreService.GetAttractionsServiceAsync(
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
     [AllowAnonymous]
     public async Task<ActionResult<ExploreAttractionDetailResponseDto>> GetAttractionControllerAsync(string id)
     {
          var attraction = await _exploreService.GetAttractionServiceAsync(id);

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
