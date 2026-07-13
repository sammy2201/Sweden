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
     [ProducesResponseType(typeof(TaxCalculationResponse), StatusCodes.Status200OK)]
     [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
     [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
     public ActionResult<TaxCalculationResponse> Calculate([FromBody] TaxCalculationRequest request)
     {
          try
          {
               var result = _taxService.Calculate(request);

               return Ok(result);
          }
          catch (ArgumentException ex)
          {
               return BadRequest(new { message = ex.Message });
          }
          catch (KeyNotFoundException ex)
          {
               return NotFound(new { message = ex.Message });
          }
     }
}
