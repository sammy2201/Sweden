using System.Globalization;
using System.Text.Json;

namespace SwedenStart;

public static class TransportMapper
{
     public static TransportTripDto? MapTrip(ResRobotTrip trip)
     {
          var firstLeg = trip.LegList?.Legs.FirstOrDefault();
          var lastLeg = trip.LegList?.Legs.LastOrDefault();

          if (firstLeg?.Origin is null || lastLeg?.Destination is null)
               return null;

          var operatorName = NormalizeOperatorName(GetOperator(firstLeg.Product));
          var departureTime = ParseDateTime(firstLeg.Origin.Date, firstLeg.Origin.Time);
          var arrivalTime = ParseDateTime(lastLeg.Destination.Date, lastLeg.Destination.Time);
          var transportType = MapTransportType(firstLeg.Product, firstLeg.Type, firstLeg.Name);
          var duration = ResolveDuration(trip.Duration, departureTime, arrivalTime);
          var numberOfChanges = ResolveNumberOfChanges(trip);
          var originPlatform = GetPlatform(firstLeg.Origin);
          var destinationPlatform = GetPlatform(lastLeg.Destination);
          var direction = ResolveDirection(firstLeg);
          var isCancelled = ResolveCancelled(trip);
          var legs = (trip.LegList?.Legs ?? [])
               .Select(MapLeg)
               .Where(x => x is not null)
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
               OriginPlatform = originPlatform,
               DestinationPlatform = destinationPlatform,
               Direction = direction,
               IsCancelled = isCancelled,
               Origin = new TransportStationDto
               {
                    Name = firstLeg.Origin.Name,
                    StationId = firstLeg.Origin.ExtId
               },
               Destination = new TransportStationDto
               {
                    Name = lastLeg.Destination.Name,
                    StationId = lastLeg.Destination.ExtId
               },
               Legs = legs
          };
     }

     private static TransportTripLegDto? MapLeg(ResRobotLeg leg)
     {
          if (leg.Origin is null || leg.Destination is null)
          {
               return null;
          }

          var originPlatform = GetPlatform(leg.Origin);

          return new TransportTripLegDto
          {
               Operator = NormalizeOperatorName(GetOperator(leg.Product)),
               TransportType = MapTransportType(leg.Product, leg.Type, leg.Name),
               Line = ResolveLine(leg),
               From = leg.Origin.Name,
               To = leg.Destination.Name,
               DepartureTime = ParseDateTime(leg.Origin.Date, leg.Origin.Time),
               ArrivalTime = ParseDateTime(leg.Destination.Date, leg.Destination.Time),
               OriginPlatform = originPlatform,
               DestinationPlatform = GetPlatform(leg.Destination),
               Direction = ResolveDirection(leg),
               IsCancelled = leg.Cancelled == true
                    || leg.Origin.Cancelled == true
                    || leg.Destination.Cancelled == true,
          };
     }

     private static string GetOperator(JsonElement product)
     {
          if (product.ValueKind == JsonValueKind.Object)
               return product.TryGetProperty("operator", out var op) ? op.GetString() ?? string.Empty : string.Empty;

          if (product.ValueKind == JsonValueKind.Array)
          {
               foreach (var item in product.EnumerateArray())
               {
                    if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("operator", out var op))
                         return op.GetString() ?? string.Empty;
               }
          }

          return string.Empty;
     }

     private static string NormalizeOperatorName(string value)
     {
          if (string.Equals(value, "Flixbus", StringComparison.OrdinalIgnoreCase))
          {
               return "FlixBus";
          }

          return value;
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

     private static string ResolveLine(ResRobotLeg leg)
     {
          if (!string.IsNullOrWhiteSpace(leg.Name))
          {
               return leg.Name;
          }

          if (leg.Product.ValueKind == JsonValueKind.Object)
          {
               return GetProductLine(leg.Product);
          }

          if (leg.Product.ValueKind == JsonValueKind.Array)
          {
               foreach (var item in leg.Product.EnumerateArray())
               {
                    if (item.ValueKind != JsonValueKind.Object)
                    {
                         continue;
                    }

                    var line = GetProductLine(item);
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                         return line;
                    }
               }
          }

          return string.Empty;
     }

     private static string GetProductLine(JsonElement product)
     {
          var line = GetJsonPropertyString(product, "line");
          if (!string.IsNullOrWhiteSpace(line))
          {
               return line;
          }

          var name = GetJsonPropertyString(product, "name");
          if (!string.IsNullOrWhiteSpace(name))
          {
               return name;
          }

          var displayNumber = GetJsonPropertyString(product, "displayNumber");
          if (!string.IsNullOrWhiteSpace(displayNumber))
          {
               return displayNumber;
          }

          return GetJsonPropertyString(product, "num");
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

          if (departureTime is DateTime dep && arrivalTime is DateTime arr && arr >= dep)
          {
               var diff = arr - dep;
               return FormatDuration(diff);
          }

          return string.Empty;
     }

     private static int ResolveNumberOfChanges(ResRobotTrip trip)
     {
          var legs = trip.LegList?.Legs ?? [];
          if (legs.Count == 0)
          {
               return Math.Max(0, trip.NumberOfChanges);
          }

          var serviceLegCount = legs.Count(IsServiceLeg);
          if (serviceLegCount > 0)
          {
               return Math.Max(0, serviceLegCount - 1);
          }

          return Math.Max(0, legs.Count - 1);
     }

     private static bool IsServiceLeg(ResRobotLeg leg)
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

     private static string GetPlatform(ResRobotStopPoint? stopPoint)
     {
          if (stopPoint is null)
          {
               return string.Empty;
          }

          if (!string.IsNullOrWhiteSpace(stopPoint.RtTrack))
          {
               return stopPoint.RtTrack;
          }

          if (!string.IsNullOrWhiteSpace(stopPoint.Track))
          {
               return stopPoint.Track;
          }

          if (!string.IsNullOrWhiteSpace(stopPoint.RtStop))
          {
               return stopPoint.RtStop;
          }

          return stopPoint.Stop;
     }

     private static string ResolveDirection(ResRobotLeg leg)
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

     private static bool ResolveCancelled(ResRobotTrip trip)
     {
          var legs = trip.LegList?.Legs ?? [];
          return legs.Any(leg => leg.Cancelled == true
               || leg.Origin?.Cancelled == true
               || leg.Destination?.Cancelled == true);
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
