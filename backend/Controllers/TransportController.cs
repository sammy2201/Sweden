using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SwedenStart;

[ApiController]
[Route("api/transport")]
[Authorize]
public class TransportController : ControllerBase
{
     private readonly ITransportService _transportService;

     public TransportController(ITransportService transportService)
     {
          _transportService = transportService;
     }

     [HttpGet("search")]
     public async Task<ActionResult<IReadOnlyList<TransportTripDto>>> SearchControllerAsync(
          [FromQuery] string from,
          [FromQuery] string to,
          [FromQuery] DateTime? departureTime = null,
          [FromQuery] DateTime? arrivalTime = null)
     {
          if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
               throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Invalid transport search",
                    "Both query params 'from' and 'to' are required.");

          try
          {
               var trips = await _transportService.SearchTripsServiceAsync(from, to, departureTime, arrivalTime);
               return Ok(trips);
          }
          catch (HttpRequestException exception)
          {
               throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "Transport service unavailable",
                    "A required external service is unavailable.",
                    innerException: exception);
          }
     }
}
