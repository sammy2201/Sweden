using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SwedenStart;

[ApiController]
[Route("api/transport")]
[Authorize]
public class TransportController : ControllerBase
{
     private readonly ITransportService _transportService;
     private readonly ILogger<TransportController> _logger;

     public TransportController(ITransportService transportService, ILogger<TransportController> logger)
     {
          _transportService = transportService;
          _logger = logger;
     }

     [HttpGet("search")]
     [ProducesResponseType(typeof(IReadOnlyList<TransportTripDto>), StatusCodes.Status200OK)]
     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
     public async Task<ActionResult<IReadOnlyList<TransportTripDto>>> Search(
          [FromQuery] string from,
          [FromQuery] string to,
          [FromQuery] DateTime? departureTime = null,
          [FromQuery] DateTime? arrivalTime = null,
          CancellationToken cancellationToken = default)
     {
          if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
               return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid transport search",
                    detail: "Both query params 'from' and 'to' are required.");

          try
          {
               var trips = await _transportService.SearchTripsAsync(from, to, departureTime, arrivalTime, cancellationToken);
               return Ok(trips);
          }
          catch (InvalidOperationException ex)
          {
               _logger.LogWarning(ex, "Transport search failed for from={From}, to={To}", from, to);
               return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "Transport service unavailable",
                    detail: ex.Message);
          }
     }
}
