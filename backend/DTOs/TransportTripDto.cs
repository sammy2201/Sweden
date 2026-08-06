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
