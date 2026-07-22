using System.Text.Json;

namespace SwedenStart;

public class TransportService : ITransportService
{
     private readonly HttpClient _httpClient;
     private readonly IConfiguration _configuration;
     private readonly ILogger<TransportService> _logger;
     private static readonly JsonSerializerOptions JsonOptions = new()
     {
          PropertyNameCaseInsensitive = true
     };

     public TransportService(
          HttpClient httpClient,
          IConfiguration configuration,
          ILogger<TransportService> logger)
     {
          _httpClient = httpClient;
          _configuration = configuration;
          _logger = logger;
     }

     public async Task<IReadOnlyList<TransportTripDto>> SearchTripsAsync(string from, string to, CancellationToken cancellationToken = default)
     {
          if (string.IsNullOrWhiteSpace(from))
               throw new ArgumentException("'from' is required.", nameof(from));

          if (string.IsNullOrWhiteSpace(to))
               throw new ArgumentException("'to' is required.", nameof(to));

          var apiKey = _configuration["ResRobot:ApiKey"];
          if (string.IsNullOrWhiteSpace(apiKey))
               throw new InvalidOperationException("ResRobot API key is not configured.");

          var origin = await LookupStationAsync(from.Trim(), apiKey, cancellationToken);
          var destination = await LookupStationAsync(to.Trim(), apiKey, cancellationToken);


          var trips = await FetchTripsAsync(origin.ExtId, destination.ExtId, apiKey, cancellationToken);
          return trips;
     }

     private async Task<ResRobotStopLocation> LookupStationAsync(string input, string apiKey, CancellationToken cancellationToken)
     {
          var endpoint = $"location.name?format=json&input={Uri.EscapeDataString(input)}&accessId={Uri.EscapeDataString(apiKey)}";
          using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

          if (!response.IsSuccessStatusCode)
          {
               var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
               _logger.LogWarning(
                    "ResRobot location lookup failed for '{Input}'. Status: {StatusCode}. Response: {Response}",
                    input,
                    (int)response.StatusCode,
                    errorBody);
               throw new InvalidOperationException("Unable to resolve station location from ResRobot.");
          }

          await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
          var payload = await JsonSerializer.DeserializeAsync<ResRobotLocationResponse>(stream, JsonOptions, cancellationToken);

          var station = payload?.StopLocationOrCoordLocation
               .Select(x => x.StopLocation)
               .FirstOrDefault(x => x is not null && !string.IsNullOrWhiteSpace(x.ExtId));

          if (station is null)
               throw new KeyNotFoundException($"No station found for '{input}'.");

          return station;
     }

     private async Task<IReadOnlyList<TransportTripDto>> FetchTripsAsync(
          string originId,
          string destinationId,
          string apiKey,
          CancellationToken cancellationToken)
     {
          var endpoint =
               $"trip?format=json&originId={Uri.EscapeDataString(originId)}&destId={Uri.EscapeDataString(destinationId)}&accessId={Uri.EscapeDataString(apiKey)}";

          using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

          if (!response.IsSuccessStatusCode)
          {
               var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
               _logger.LogWarning(
                    "ResRobot trip lookup failed. OriginId: {OriginId}, DestId: {DestId}, Status: {StatusCode}, Response: {Response}",
                    originId,
                    destinationId,
                    (int)response.StatusCode,
                    errorBody);
               throw new InvalidOperationException("Unable to fetch trips from ResRobot.");
          }

          await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
          var payload = await JsonSerializer.DeserializeAsync<ResRobotTripResponse>(stream, JsonOptions, cancellationToken);

          var result = (payload?.Trips ?? [])
               .Select(TransportMapper.MapTrip)
               .Where(x => x is not null)
               .Cast<TransportTripDto>()
               .ToList();

          return result;
     }
}
