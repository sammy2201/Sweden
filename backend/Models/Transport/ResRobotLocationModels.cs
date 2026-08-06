using System.Text.Json.Serialization;

namespace SwedenStart;

public class ResRobotLocationResponse
{
     [JsonPropertyName("stopLocationOrCoordLocation")]
     public List<ResRobotLocationWrapper> StopLocationOrCoordLocation { get; set; } = [];
}

public class ResRobotLocationWrapper
{
     [JsonPropertyName("StopLocation")]
     public ResRobotStopLocation? StopLocation { get; set; }
}

public class ResRobotStopLocation
{
     [JsonPropertyName("extId")]
     public string ExtId { get; set; } = string.Empty;

     [JsonPropertyName("name")]
     public string Name { get; set; } = string.Empty;
}