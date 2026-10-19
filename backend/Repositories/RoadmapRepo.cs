using Microsoft.EntityFrameworkCore;

namespace SwedenStart;

public class RoadmapRepository : IRoadmapRepository
{
    private readonly AppDbContext _dbContext;

    public RoadmapRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Roadmap> SaveRoadmapRepoAsync(Roadmap roadmap)
    {
        await _dbContext.Roadmaps.AddAsync(roadmap);
        await _dbContext.SaveChangesAsync();
        return roadmap;
    }


    public async Task<Roadmap?> GetRoadmapByUserIdRepoAsync(Guid userId)
    {
        return await _dbContext.Roadmaps
            .Include(roadmap => roadmap.Tasks)
            .FirstOrDefaultAsync(roadmap => roadmap.UserId == userId);
    }

    public async Task SaveRoadmapChangesRepoAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> DeleteRoadmapRepoAsync(Guid userId)
    {
        var existing = await _dbContext.Roadmaps
            .FirstOrDefaultAsync(roadmap => roadmap.UserId == userId);

        if (existing is null)
        {
            return false;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        await _dbContext.RoadmapTasks
            .Where(task => task.RoadmapId == existing.Id)
            .ExecuteDeleteAsync();

        _dbContext.Roadmaps.Remove(existing);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

}
