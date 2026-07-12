using Microsoft.AspNetCore.Mvc;
namespace SwedenStart;



[ApiController]
[Route("api/tax")]
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
     public IActionResult Calculate([FromBody] TaxCalculationRequest request)
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
