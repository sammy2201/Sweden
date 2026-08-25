namespace SwedenStart;

public interface IRoadmapService
{
    Task<RoadmapResponseDto> GenerateRoadmapAsync(RoadmapRequestDto request, Guid userId);
    Task<RoadmapResponseDto> GetRoadmapAsync(Guid userId);
    Task<RoadmapResponseDto> UpdateRoadmapAsync(RoadmapRequestDto request, Guid userId);
}