using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace SwedenStart;


[ApiController]
[Route("api/bank-details")]
[Authorize]
public class BankController : ControllerBase
{
     private readonly IBankService _bankService;

     public BankController(IBankService bankService)
     {
          _bankService = bankService;
     }

     [HttpGet]
     public ActionResult<IEnumerable<BankDto>> GetBanksController()
     {
          return Ok(_bankService.GetBanksService());
     }
}
