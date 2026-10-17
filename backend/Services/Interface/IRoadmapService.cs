namespace SwedenStart;

public interface IRoadmapService
{
    Task<RoadmapResponseDto> GenerateRoadmapAsync(RoadmapRequestDto request, Guid userId);
    Task<RoadmapResponseDto?> GetRoadmapAsync(Guid userId);
    Task<RoadmapResponseDto> UpdateRoadmapAsync(RoadmapRequestDto request, Guid userId);
    Task<bool> DeleteRoadmapAsync(Guid userId);
    Task<RoadmapResponseDto> SetTaskCompletionAsync(Guid userId, Guid taskId, bool completed);
}