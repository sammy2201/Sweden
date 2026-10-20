namespace SwedenStart;

public interface ITransportService
{
     Task<IReadOnlyList<TransportTripDto>> SearchTripsServiceAsync(
          string from,
          string to,
          DateTime? departureTime = null,
          DateTime? arrivalTime = null);
}
