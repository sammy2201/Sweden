using Microsoft.AspNetCore.Mvc;

namespace SwedenStart;

[ApiController]
[Route("api/visitsweden")]
public class VisitSwedenController : ControllerBase
{
     private readonly IVisitSwedenService _visitSwedenService;

     public VisitSwedenController(IVisitSwedenService visitSwedenService)
     {
          _visitSwedenService = visitSwedenService;
     }

     [HttpPost("sync")]
     public async Task<IActionResult> Sync()
     {
          await _visitSwedenService.SyncAttractionsAsync();
          return Ok(new { message = "Sync completed." });
     }
}