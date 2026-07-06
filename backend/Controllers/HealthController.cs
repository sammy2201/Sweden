using Microsoft.AspNetCore.Mvc;
namespace SwedenStart.Health;

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
     public async Task<ActionResult> GetHealth()
     {
       var response = await _healthService.GetHealthAsync();
        return Ok(response);
     }
}