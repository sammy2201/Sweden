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
     public async Task<ActionResult<ExploreCountiesResponseDto>> GetCounties()
     {
          var counties = await _exploreService.GetCountiesAsync();

          return Ok(new ExploreCountiesResponseDto
          {
               Counties = counties.ToList()
          });
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