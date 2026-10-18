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

     public IActionResult GetRates()
     {
          return Ok(_taxService.GetTaxRates());
     }

     [HttpPost("calculate")]
     public ActionResult<TaxCalculationResponse> Calculate([FromBody] TaxCalculationRequest request)
     {
          var result = _taxService.Calculate(request);
          return Ok(result);
     }

     [HttpGet("municipality")]
     public IActionResult GetMunicipalities()
     {
          return Ok(_taxService.GetMunicipalities());
     }

}
