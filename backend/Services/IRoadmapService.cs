namespace SwedenStart.Health;

public interface IRoadmapService
{
    Task<RoadmapResponseDto> GenerateRoadmapAsync(RoadmapRequestDto request);
}