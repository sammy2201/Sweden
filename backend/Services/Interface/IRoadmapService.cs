namespace SwedenStart;

public interface IRoadmapService
{
    Task<RoadmapResponseDto> GenerateRoadmapServiceAsync(RoadmapRequestDto request, Guid userId);
    Task<RoadmapResponseDto?> GetRoadmapServiceAsync(Guid userId);
    Task<RoadmapResponseDto> UpdateRoadmapServiceAsync(RoadmapRequestDto request, Guid userId);
    Task<bool> DeleteRoadmapServiceAsync(Guid userId);
    Task<RoadmapResponseDto> SetTaskCompletionServiceAsync(Guid userId, Guid taskId, bool completed);
}