namespace SwedenStart;

public interface IRoadmapRepository
{
    Task<Roadmap> SaveAsync(Roadmap roadmap);
    Task<Roadmap?> GetByUserIdAsync(Guid userId);
    Task<Roadmap> UpdateAsync(Roadmap roadmap);
}
