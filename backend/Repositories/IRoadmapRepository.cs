namespace SwedenStart.Health;

public interface IRoadmapRepository
{
    Task<Roadmap> SaveAsync(Roadmap roadmap);
}
