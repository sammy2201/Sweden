namespace SwedenStart;

public interface IRoadmapRepository
{
    Task<Roadmap> SaveRoadmapRepoAsync(Roadmap roadmap);
    Task<Roadmap?> GetRoadmapByUserIdRepoAsync(Guid userId);
    Task SaveRoadmapChangesRepoAsync();
    Task<bool> DeleteRoadmapRepoAsync(Guid userId);
}
