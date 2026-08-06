using System.Text.Json;
using System.Text.Json.Serialization;

namespace SwedenStart;

public class ResRobotTripResponse
{
     [JsonPropertyName("Trip")]
     public List<ResRobotTrip> Trips { get; set; } = [];
}

public class ResRobotTrip
{
     [JsonPropertyName("dur")]
     public string Duration { get; set; } = string.Empty;

     [JsonPropertyName("chg")]
     [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
     public int NumberOfChanges { get; set; }

     [JsonPropertyName("LegList")]
     public ResRobotLegList? LegList { get; set; }
}

public class ResRobotLegList
{
     [JsonPropertyName("Leg")]
     public List<ResRobotLeg> Legs { get; set; } = [];
}

public class ResRobotLeg
{
     [JsonPropertyName("Origin")]
     public ResRobotStopPoint? Origin { get; set; }

     [JsonPropertyName("Destination")]
     public ResRobotStopPoint? Destination { get; set; }

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

public class ResRobotStopPoint
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