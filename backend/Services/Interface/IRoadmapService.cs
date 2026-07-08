namespace SwedenStart;

public interface IRoadmapService
{
    Task<RoadmapResponseDto> GenerateRoadmapAsync(RoadmapRequestDto request, Guid userId);
}