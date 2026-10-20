using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace SwedenStart;



[ApiController]
[Route("api/tax")]
[Authorize]
public class TaxController : ControllerBase
{
     private readonly ITaxService _taxService;

     public TaxController(ITaxService taxService)
     {
          _taxService = taxService;
     }

     [HttpGet("rates")]

     public IActionResult GetRatesController()
     {
          return Ok(_taxService.GetTaxRatesService());
     }

     [HttpPost("calculate")]
     public ActionResult<TaxCalculationResponse> CalculateController([FromBody] TaxCalculationRequest request)
     {
          var result = _taxService.CalculateService(request);
          return Ok(result);
     }

     [HttpGet("municipality")]
     public IActionResult GetMunicipalitiesController()
     {
          return Ok(_taxService.GetMunicipalitiesService());
     }

}
