namespace SwedenStart;

public interface IRoadmapRepository
{
    Task<Roadmap> SaveAsync(Roadmap roadmap);
}
