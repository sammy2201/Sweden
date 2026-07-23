namespace SwedenStart;

public interface ITransportService
{
     Task<IReadOnlyList<TransportTripDto>> SearchTripsAsync(
          string from,
          string to,
          DateTime? departureTime = null,
          DateTime? arrivalTime = null,
          CancellationToken cancellationToken = default);
}
