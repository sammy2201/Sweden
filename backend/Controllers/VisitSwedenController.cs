using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SwedenStart;

[ApiController]
[Route("api/visitsweden")]
[Authorize]
public class VisitSwedenController : ControllerBase
{
     private readonly IVisitSwedenService _visitSwedenService;

     public VisitSwedenController(IVisitSwedenService visitSwedenService)
     {
          _visitSwedenService = visitSwedenService;
     }

     [HttpPost("sync")]
     [Authorize(Roles = "Admin")]
     public async Task<ActionResult<VisitSwedenSyncResult>> Sync(CancellationToken cancellationToken)
     {
          try
          {
               var result = await _visitSwedenService.SyncAttractionsAsync(cancellationToken);
               return Ok(result);
          }
          catch (HttpRequestException exception)
          {
               throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "External service unavailable",
                    "A required external service is unavailable.",
                    innerException: exception);
          }
     }
}
