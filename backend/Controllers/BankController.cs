using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


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
     public async Task<ActionResult<IEnumerable<BankDto>>> GetBanks()
     {
          var banks = await _bankService.GetBanksAsync();
          return Ok(banks);
     }
}