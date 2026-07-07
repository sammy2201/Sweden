using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SwedenStart;

public class RoadmapRepository : IRoadmapRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RoadmapRepository> _logger;

    public RoadmapRepository(AppDbContext dbContext, ILogger<RoadmapRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
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
