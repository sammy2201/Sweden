
using System.Globalization;
using System.Text.Json;

namespace SwedenStart;

public static class TransportMapper
{
     public static TransportTripDto? MapTrip(ResRobotTripDto trip)
     {
          var legs = trip.LegList?.Legs ?? [];
          var firstLeg = legs.FirstOrDefault();
          var lastLeg = legs.LastOrDefault();

          if (firstLeg?.Origin == null || lastLeg?.Destination == null)
               return null;

          var departure = ParseDateTime(
              firstLeg.Origin.Date, firstLeg.Origin.Time);

          var arrival = ParseDateTime(
              lastLeg.Destination.Date, lastLeg.Destination.Time);

          var mappedLegs = new List<TransportTripLegDto>();

          foreach (var leg in legs)
          {
               var mappedLeg = MapLeg(leg);

               if (mappedLeg != null)
                    mappedLegs.Add(mappedLeg);
          }

          return new TransportTripDto
          {
               Operator = GetOperator(firstLeg.Product),
               DepartureTime = departure,
               ArrivalTime = arrival,
               Duration = GetDuration(trip.Duration, departure, arrival),
               NumberOfChanges = Math.Max(0, trip.NumberOfChanges),
               IsDirect = trip.NumberOfChanges == 0,
               TransportType = GetTransportType(firstLeg),
               Direction = firstLeg.Direction,
               IsCancelled = mappedLegs.Any(leg => leg.IsCancelled),

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

               Legs = mappedLegs
          };
     }

     private static TransportTripLegDto? MapLeg(ResRobotLegDto leg)
     {
          if (leg.Origin == null || leg.Destination == null)
               return null;

          return new TransportTripLegDto
          {
               Operator = GetOperator(leg.Product),
               TransportType = GetTransportType(leg),
               From = leg.Origin.Name,
               To = leg.Destination.Name,
               DepartureTime = ParseDateTime(
                  leg.Origin.Date, leg.Origin.Time),
               ArrivalTime = ParseDateTime(
                  leg.Destination.Date, leg.Destination.Time),
               Direction = leg.Direction,
               IsCancelled = leg.Cancelled == true
                  || leg.Origin.Cancelled == true
                  || leg.Destination.Cancelled == true
          };
     }

     private static string GetOperator(JsonElement product)
     {
          if (product.ValueKind == JsonValueKind.Array)
               product = product.EnumerateArray().FirstOrDefault();

          if (product.ValueKind == JsonValueKind.Object
              && product.TryGetProperty("operator", out var value))
          {
               return value.GetString() ?? string.Empty;
          }

          return string.Empty;
     }

     private static string GetTransportType(ResRobotLegDto leg)
     {
          var type = leg.Type.ToLowerInvariant();
          var name = leg.Name.ToLowerInvariant();

          if (type == "walk")
               return "Walk";

          if (type == "trsf" || type == "transfer")
               return "Transfer";

          if (name.Contains("metro") || name.Contains("tunnelbana"))
               return "Metro";

          if (name.Contains("tram") || name.Contains("spårvagn"))
               return "Tram";

          if (name.Contains("ferry") || name.Contains("färja"))
               return "Ferry";

          if (name.Contains("bus"))
               return "Bus";

          return "Train";
     }

     private static string GetDuration(
         string apiDuration,
         DateTime? departure,
         DateTime? arrival)
     {
          if (!string.IsNullOrWhiteSpace(apiDuration))
               return apiDuration;

          if (departure.HasValue && arrival.HasValue
              && arrival.Value >= departure.Value)
          {
               return (arrival.Value - departure.Value).ToString(@"hh\:mm");
          }

          return string.Empty;
     }

     private static DateTime? ParseDateTime(string date, string time)
     {
          if (string.IsNullOrWhiteSpace(date)
              || string.IsNullOrWhiteSpace(time))
               return null;

          return DateTime.TryParseExact(
              $"{date} {time}",
              "yyyy-MM-dd HH:mm:ss",
              CultureInfo.InvariantCulture,
              DateTimeStyles.AssumeLocal,
              out var result)
                  ? result
                  : null;
     }
}
