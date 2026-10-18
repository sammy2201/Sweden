using System.Globalization;
using System.Text.Json;

namespace SwedenStart;

public static class TransportMapper
{
     public static TransportTripDto? MapTrip(ResRobotTripDto trip)
     {
          var firstSegment = trip.LegList?.Legs.FirstOrDefault();
          var lastSegment = trip.LegList?.Legs.LastOrDefault();

          if (firstSegment?.Origin is null || lastSegment?.Destination is null)
               return null;

          var operatorName = GetOperator(firstSegment.Product);
          var departureTime = ParseDateTime(firstSegment.Origin.Date, firstSegment.Origin.Time);
          var arrivalTime = ParseDateTime(lastSegment.Destination.Date, lastSegment.Destination.Time);
          var transportType = MapTransportType(firstSegment.Product, firstSegment.Type, firstSegment.Name);
          var duration = ResolveDuration(trip.Duration, departureTime, arrivalTime);
          var numberOfChanges = ResolveNumberOfChanges(trip);
          var direction = ResolveDirection(firstSegment);
          var isCancelled = ResolveCancelled(trip);
          var segments = (trip.LegList?.Legs ?? [])
               .Select(MapSegment)
               .Where(segment => segment is not null)
               .Cast<TransportTripLegDto>()
               .ToList();

          return new TransportTripDto
          {
               Operator = operatorName,
               DepartureTime = departureTime,
               ArrivalTime = arrivalTime,
               Duration = duration,
               NumberOfChanges = numberOfChanges,
               IsDirect = numberOfChanges == 0,
               TransportType = transportType,
               Direction = direction,
               IsCancelled = isCancelled,
               Origin = new TransportStationDto
               {
                    Name = firstSegment.Origin.Name,
                    StationId = firstSegment.Origin.ExtId
               },
               Destination = new TransportStationDto
               {
                    Name = lastSegment.Destination.Name,
                    StationId = lastSegment.Destination.ExtId
               },
               Legs = segments
          };
     }

     private static TransportTripLegDto? MapSegment(ResRobotLegDto segment)
     {
          if (segment.Origin is null || segment.Destination is null)
          {
               return null;
          }

          return new TransportTripLegDto
          {
               Operator = GetOperator(segment.Product),
               TransportType = MapTransportType(segment.Product, segment.Type, segment.Name),
               From = segment.Origin.Name,
               To = segment.Destination.Name,
               DepartureTime = ParseDateTime(segment.Origin.Date, segment.Origin.Time),
               ArrivalTime = ParseDateTime(segment.Destination.Date, segment.Destination.Time),
               Direction = ResolveDirection(segment),
               IsCancelled = segment.Cancelled == true
                    || segment.Origin.Cancelled == true
                    || segment.Destination.Cancelled == true,
          };
     }

     private static string GetOperator(JsonElement product)
     {
          if (product.ValueKind == JsonValueKind.Object)
               return product.TryGetProperty("operator", out var operatorElement) ? operatorElement.GetString() ?? string.Empty : string.Empty;

          if (product.ValueKind == JsonValueKind.Array)
          {
               foreach (var item in product.EnumerateArray())
               {
                    if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("operator", out var operatorElement))
                         return operatorElement.GetString() ?? string.Empty;
               }
          }

          return string.Empty;
     }


     private static string MapTransportType(JsonElement product, string legType, string legName)
     {
          var candidates = new List<string>();
          var normalizedLegType = legType?.Trim() ?? string.Empty;

          if (normalizedLegType.Equals("WALK", StringComparison.OrdinalIgnoreCase))
               return "Walk";

          if (normalizedLegType.Equals("TRSF", StringComparison.OrdinalIgnoreCase)
               || normalizedLegType.Equals("TRANSFER", StringComparison.OrdinalIgnoreCase))
               return "Transfer";

          if (!string.IsNullOrWhiteSpace(legType))
          {
               candidates.Add(legType);
          }

          if (!string.IsNullOrWhiteSpace(legName))
          {
               candidates.Add(legName);
          }

          if (product.ValueKind == JsonValueKind.Object)
          {
               AddJsonString(product, "catOut", candidates);
               AddJsonString(product, "catIn", candidates);
               AddJsonString(product, "name", candidates);
               AddJsonString(product, "displayNumber", candidates);
          }

          if (product.ValueKind == JsonValueKind.Array)
          {
               foreach (var item in product.EnumerateArray())
               {
                    if (item.ValueKind != JsonValueKind.Object)
                    {
                         continue;
                    }

                    AddJsonString(item, "catOut", candidates);
                    AddJsonString(item, "catIn", candidates);
                    AddJsonString(item, "name", candidates);
                    AddJsonString(item, "displayNumber", candidates);
               }
          }

          var joined = string.Join(" ", candidates).ToLowerInvariant();

          if (joined.Contains("metro") || joined.Contains("subway") || joined.Contains("tunnelbana"))
               return "Metro";

          if (joined.Contains("tram") || joined.Contains("sp\u00e5rvagn") || joined.Contains("spv"))
               return "Tram";

          if (joined.Contains("ferry") || joined.Contains("boat") || joined.Contains("ship") || joined.Contains("f\u00e4rja"))
               return "Ferry";

          if (joined.Contains("bus"))
               return "Bus";

          if (joined.Contains("train") || joined.Contains("rail") || joined.Contains("t\u00e5g") || joined.Contains("jny"))
               return "Train";

          return "Train";
     }

     private static void AddJsonString(JsonElement source, string propertyName, List<string> destination)
     {
          if (source.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
          {
               var text = value.GetString();
               if (!string.IsNullOrWhiteSpace(text))
               {
                    destination.Add(text);
               }
          }
     }

     private static string ResolveDuration(string apiDuration, DateTime? departureTime, DateTime? arrivalTime)
     {
          if (!string.IsNullOrWhiteSpace(apiDuration))
          {
               return apiDuration;
          }

          if (departureTime is DateTime departureDateTime && arrivalTime is DateTime arrivalDateTime && arrivalDateTime >= departureDateTime)
          {
               var duration = arrivalDateTime - departureDateTime;
               return FormatDuration(duration);
          }

          return string.Empty;
     }

     private static int ResolveNumberOfChanges(ResRobotTripDto trip)
     {
          var segments = trip.LegList?.Legs ?? [];
          if (segments.Count == 0)
          {
               return Math.Max(0, trip.NumberOfChanges);
          }

          var serviceSegmentCount = segments.Count(IsServiceLeg);
          if (serviceSegmentCount > 0)
          {
               return Math.Max(0, serviceSegmentCount - 1);
          }

          return Math.Max(0, segments.Count - 1);
     }

     private static bool IsServiceLeg(ResRobotLegDto leg)
     {
          if (leg.Product.ValueKind == JsonValueKind.Object || leg.Product.ValueKind == JsonValueKind.Array)
          {
               return true;
          }

          var type = leg.Type?.Trim() ?? string.Empty;
          if (type.Equals("WALK", StringComparison.OrdinalIgnoreCase)
               || type.Equals("TRSF", StringComparison.OrdinalIgnoreCase)
               || type.Equals("TRANSFER", StringComparison.OrdinalIgnoreCase))
          {
               return false;
          }

          return true;
     }


     private static string ResolveDirection(ResRobotLegDto leg)
     {
          if (!string.IsNullOrWhiteSpace(leg.Direction))
          {
               return leg.Direction;
          }

          if (leg.Product.ValueKind == JsonValueKind.Object)
          {
               return GetJsonPropertyString(leg.Product, "direction");
          }

          if (leg.Product.ValueKind == JsonValueKind.Array)
          {
               foreach (var item in leg.Product.EnumerateArray())
               {
                    if (item.ValueKind != JsonValueKind.Object)
                    {
                         continue;
                    }

                    var direction = GetJsonPropertyString(item, "direction");
                    if (!string.IsNullOrWhiteSpace(direction))
                    {
                         return direction;
                    }
               }
          }

          return string.Empty;
     }

     private static bool ResolveCancelled(ResRobotTripDto trip)
     {
          var segments = trip.LegList?.Legs ?? [];
          return segments.Any(segment => segment.Cancelled == true
               || segment.Origin?.Cancelled == true
               || segment.Destination?.Cancelled == true);
     }

     private static string GetJsonPropertyString(JsonElement source, string propertyName)
     {
          if (source.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
          {
               return value.GetString() ?? string.Empty;
          }

          return string.Empty;
     }

     private static string FormatDuration(TimeSpan duration)
     {
          var totalHours = (int)duration.TotalHours;
          return $"{totalHours:D2}:{duration.Minutes:D2}";
     }

     private static DateTime? ParseDateTime(string date, string time)
     {
          if (string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(time))
               return null;

          var value = $"{date} {time}";
          return DateTime.TryParseExact(
               value,
               "yyyy-MM-dd HH:mm:ss",
               CultureInfo.InvariantCulture,
               DateTimeStyles.AssumeLocal,
               out var parsed)
               ? parsed
               : null;
     }
}
