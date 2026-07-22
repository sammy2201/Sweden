namespace SwedenStart;

public interface ITransportService
{
     Task<IReadOnlyList<TransportTripDto>> SearchTripsAsync(string from, string to, CancellationToken cancellationToken = default);
}
