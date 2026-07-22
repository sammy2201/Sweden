using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SwedenStart;

[ApiController]
[Route("api/transport")]
[AllowAnonymous]
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
     [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
     [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
     [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
     public async Task<ActionResult<IReadOnlyList<TransportTripDto>>> Search(
          [FromQuery] string from,
          [FromQuery] string to,
          CancellationToken cancellationToken)
     {
          if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
               return BadRequest(new { message = "Both query params 'from' and 'to' are required." });

          try
          {
               var trips = await _transportService.SearchTripsAsync(from, to, cancellationToken);
               return Ok(trips);
          }
          catch (ArgumentException ex)
          {
               return BadRequest(new { message = ex.Message });
          }
          catch (KeyNotFoundException ex)
          {
               return NotFound(new { message = ex.Message });
          }
          catch (InvalidOperationException ex)
          {
               _logger.LogWarning(ex, "Transport search failed for from={From}, to={To}", from, to);
               return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Unhandled error during transport search for from={From}, to={To}", from, to);
               return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error while searching transport trips." });
          }
     }
}
