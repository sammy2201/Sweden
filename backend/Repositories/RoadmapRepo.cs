using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SwedenStart;

public class RoadmapRepository : IRoadmapRepository
{
    private readonly AppDbContext _dbContext;


    public RoadmapRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Roadmap> SaveAsync(Roadmap roadmap)
    {
        roadmap.Id = roadmap.Id == Guid.Empty ? Guid.NewGuid() : roadmap.Id;
        roadmap.CreatedAt = roadmap.CreatedAt == default ? DateTime.UtcNow : roadmap.CreatedAt;

        foreach (var task in roadmap.Tasks)
        {
            task.Id = task.Id == Guid.Empty ? Guid.NewGuid() : task.Id;
        }

        await _dbContext.Roadmaps.AddAsync(roadmap);
        await _dbContext.SaveChangesAsync();

        return roadmap;
    }
}
