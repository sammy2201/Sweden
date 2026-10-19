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
     public async Task<ActionResult<VisitSwedenSyncResult>> SyncControllerAsync()
     {
          var result = await _visitSwedenService.SyncAttractionsServiceAsync();
          return Ok(result);
     }
}
