using System.Text.Json;
using System.Text.Json.Serialization;

namespace SwedenStart;

public class TransportTripDto
{
     public string Operator { get; set; } = string.Empty;
     public DateTime? DepartureTime { get; set; }
     public DateTime? ArrivalTime { get; set; }
     public string Duration { get; set; } = string.Empty;
     public int NumberOfChanges { get; set; }
     public bool IsDirect { get; set; }
     public string TransportType { get; set; } = string.Empty;
     public string OriginPlatform { get; set; } = string.Empty;
     public string DestinationPlatform { get; set; } = string.Empty;
     public string Direction { get; set; } = string.Empty;
     public bool IsCancelled { get; set; }
     public TransportStationDto Origin { get; set; } = new();
     public TransportStationDto Destination { get; set; } = new();
     public IReadOnlyList<TransportTripLegDto> Legs { get; set; } = new List<TransportTripLegDto>();
}

public class TransportStationDto
{
     public string Name { get; set; } = string.Empty;
     public string StationId { get; set; } = string.Empty;
}

public class TransportTripLegDto
{
     public string Operator { get; set; } = string.Empty;
     public string TransportType { get; set; } = string.Empty;
     public string Line { get; set; } = string.Empty;
     public string From { get; set; } = string.Empty;
     public string To { get; set; } = string.Empty;
     public DateTime? DepartureTime { get; set; }
     public DateTime? ArrivalTime { get; set; }
     public string OriginPlatform { get; set; } = string.Empty;
     public string DestinationPlatform { get; set; } = string.Empty;
     public string Direction { get; set; } = string.Empty;
     public bool IsCancelled { get; set; }

}

public class ResRobotLocationResponseDto
{
     [JsonPropertyName("stopLocationOrCoordLocation")]
     public List<ResRobotLocationWrapperDto> StopLocationOrCoordLocation { get; set; } = [];
}

public class ResRobotLocationWrapperDto
{
     [JsonPropertyName("StopLocation")]
     public ResRobotStopLocationDto? StopLocation { get; set; }
}

public class ResRobotStopLocationDto
{
     [JsonPropertyName("extId")]
     public string ExtId { get; set; } = string.Empty;

     [JsonPropertyName("name")]
     public string Name { get; set; } = string.Empty;
}

public class ResRobotTripResponseDto
{
     [JsonPropertyName("Trip")]
     public List<ResRobotTripDto> Trips { get; set; } = [];
}

public class ResRobotTripDto
{
     [JsonPropertyName("dur")]
     public string Duration { get; set; } = string.Empty;

     [JsonPropertyName("chg")]
     [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
     public int NumberOfChanges { get; set; }

     [JsonPropertyName("LegList")]
     public ResRobotLegListDto? LegList { get; set; }
}

public class ResRobotLegListDto
{
     [JsonPropertyName("Leg")]
     public List<ResRobotLegDto> Legs { get; set; } = [];
}

public class ResRobotLegDto
{
     [JsonPropertyName("Origin")]
     public ResRobotStopPointDto? Origin { get; set; }

     [JsonPropertyName("Destination")]
     public ResRobotStopPointDto? Destination { get; set; }

     [JsonPropertyName("Product")]
     public JsonElement Product { get; set; }

     [JsonPropertyName("type")]
     public string Type { get; set; } = string.Empty;

     [JsonPropertyName("name")]
     public string Name { get; set; } = string.Empty;

     [JsonPropertyName("direction")]
     public string Direction { get; set; } = string.Empty;

     [JsonPropertyName("cancelled")]
     public bool? Cancelled { get; set; }
}

public class ResRobotStopPointDto
{
     [JsonPropertyName("name")]
     public string Name { get; set; } = string.Empty;

     [JsonPropertyName("extId")]
     public string ExtId { get; set; } = string.Empty;

     [JsonPropertyName("date")]
     public string Date { get; set; } = string.Empty;

     [JsonPropertyName("time")]
     public string Time { get; set; } = string.Empty;

     [JsonPropertyName("track")]
     public string Track { get; set; } = string.Empty;

     [JsonPropertyName("rtTrack")]
     public string RtTrack { get; set; } = string.Empty;

     [JsonPropertyName("stop")]
     public string Stop { get; set; } = string.Empty;

     [JsonPropertyName("rtStop")]
     public string RtStop { get; set; } = string.Empty;

     [JsonPropertyName("cancelled")]
     public bool? Cancelled { get; set; }
}
