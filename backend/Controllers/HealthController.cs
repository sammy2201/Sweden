using Microsoft.AspNetCore.Mvc;
namespace SwedenStart;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{

   private readonly IHealthService _healthService;

   public HealthController(IHealthService healthService)
   {
      _healthService = healthService;
   }

   [HttpGet]
   public async Task<ActionResult> GetHealthControllerAsync()
   {
      var response = await _healthService.GetHealthServiceAsync();
      return Ok(response);
   }
}