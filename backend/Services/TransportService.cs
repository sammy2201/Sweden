using System.Text.Json;

namespace SwedenStart;

public class TransportService : ITransportService
{
     private readonly HttpClient _httpClient;
     private readonly IConfiguration _config;
     private readonly ILogger<TransportService> _logger;
     private static readonly JsonSerializerOptions JsonOptions = new()
     {
          PropertyNameCaseInsensitive = true
     };

     public TransportService(
          HttpClient httpClient,
          IConfiguration config,
          ILogger<TransportService> logger)
     {
          _httpClient = httpClient;
          _config = config;
          _logger = logger;
     }

     public async Task<IReadOnlyList<TransportTripDto>> SearchTripsServiceAsync(
          string from,
          string to,
          DateTime? departureTime = null,
          DateTime? arrivalTime = null)
     {
          if (string.IsNullOrWhiteSpace(from))
               throw new ApiException(StatusCodes.Status400BadRequest, "Invalid request", "'from' is required.");

          if (string.IsNullOrWhiteSpace(to))
               throw new ApiException(StatusCodes.Status400BadRequest, "Invalid request", "'to' is required.");

          var apiKey = _config["ResRobot:ApiKey"];
          if (string.IsNullOrWhiteSpace(apiKey))
               throw new ApiException(StatusCodes.Status502BadGateway, "Transport service unavailable", "ResRobot API key is not configured.");

          var origin = await LookupStationAsync(from.Trim(), apiKey);
          var destination = await LookupStationAsync(to.Trim(), apiKey);
          var trips = await FetchTripsAsync(origin.ExtId, destination.ExtId, apiKey, departureTime, arrivalTime);
          return trips;
     }

     private async Task<ResRobotStopLocationDto> LookupStationAsync(string input, string apiKey)
     {
          var endpoint = $"location.name?format=json&input={Uri.EscapeDataString(input)}&accessId={Uri.EscapeDataString(apiKey)}";
          using var response = await _httpClient.GetAsync(endpoint);

          if (!response.IsSuccessStatusCode)
          {
               var errorBody = await response.Content.ReadAsStringAsync();
               _logger.LogWarning(
                    "ResRobot location lookup failed for '{Input}'. Status: {StatusCode}. Response: {Response}",
                    input,
                    (int)response.StatusCode,
                    errorBody);
               throw new ApiException(StatusCodes.Status502BadGateway, "Transport service unavailable", "Unable to resolve station location from ResRobot.");
          }

          await using var stream = await response.Content.ReadAsStreamAsync();
          var payload = await JsonSerializer.DeserializeAsync<ResRobotLocationResponseDto>(stream, JsonOptions);

          var station = payload?.StopLocationOrCoordLocation
               .Select(locationResult => locationResult.StopLocation)
               .FirstOrDefault(stopLocation => stopLocation is not null && !string.IsNullOrWhiteSpace(stopLocation.ExtId));

          if (station is null)
               throw new ApiException(StatusCodes.Status404NotFound, "Resource not found", $"No station found for '{input}'.");

          return station;
     }

     private async Task<IReadOnlyList<TransportTripDto>> FetchTripsAsync(
          string originId,
          string destinationId,
          string apiKey,
          DateTime? departureTime = null,
          DateTime? arrivalTime = null)
     {
          var queryParts = new List<string>
          {
               "format=json",
               $"originId={Uri.EscapeDataString(originId)}",
               $"destId={Uri.EscapeDataString(destinationId)}",
               $"accessId={Uri.EscapeDataString(apiKey)}"
          };

          // ResRobot uses date/time as the search anchor (departure by default, arrival when requested).
          if (arrivalTime.HasValue)
          {
               queryParts.Add($"date={arrivalTime.Value:yyyy-MM-dd}");
               queryParts.Add($"time={arrivalTime.Value:HH:mm}");
               queryParts.Add("searchForArrival=1");
          }
          else if (departureTime.HasValue)
          {
               queryParts.Add($"date={departureTime.Value:yyyy-MM-dd}");
               queryParts.Add($"time={departureTime.Value:HH:mm}");
          }

          var endpoint = $"trip?{string.Join("&", queryParts)}";

          using var response = await _httpClient.GetAsync(endpoint);

          if (!response.IsSuccessStatusCode)
          {
               var errorBody = await response.Content.ReadAsStringAsync();
               _logger.LogWarning(
                    "ResRobot trip lookup failed. OriginId: {OriginId}, DestId: {DestId}, Status: {StatusCode}, Response: {Response}",
                    originId,
                    destinationId,
                    (int)response.StatusCode,
                    errorBody);
               throw new ApiException(StatusCodes.Status502BadGateway, "Transport service unavailable", "Unable to fetch trips from ResRobot.");
          }

          await using var stream = await response.Content.ReadAsStreamAsync();
          var payload = await JsonSerializer.DeserializeAsync<ResRobotTripResponseDto>(stream, JsonOptions);

          var result = (payload?.Trips ?? [])
               .Select(TransportMapper.MapTrip)
               .Where(trip => trip is not null)
               .Cast<TransportTripDto>()
               .ToList();

          return result;
     }
}
