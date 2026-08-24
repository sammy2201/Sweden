using Microsoft.EntityFrameworkCore;


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
        var existing = await _dbContext.Roadmaps
            .Include(r => r.Tasks)
            .FirstOrDefaultAsync(r => r.UserId == roadmap.UserId);

        if (existing is null)
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

        existing.Origin = roadmap.Origin;
        existing.ResidencePermit = roadmap.ResidencePermit;
        existing.LiveInSweden = roadmap.LiveInSweden;
        existing.Personnummer = roadmap.Personnummer;
        existing.AppliedPersonnummer = roadmap.AppliedPersonnummer;
        existing.IdCard = roadmap.IdCard;
        existing.BankAccount = roadmap.BankAccount;
        existing.BankId = roadmap.BankId;
        existing.Housing = roadmap.Housing;
        existing.PlanToDrive = roadmap.PlanToDrive;
        existing.DrivingLicenceType = roadmap.DrivingLicenceType;
        existing.Purpose = roadmap.Purpose;
        existing.Insurance = roadmap.Insurance;

        _dbContext.RoadmapTasks.RemoveRange(existing.Tasks);

        var replacementTasks = roadmap.Tasks.Select(task => new RoadmapTask
        {
            Id = task.Id == Guid.Empty ? Guid.NewGuid() : task.Id,
            Title = task.Title,
            Description = task.Description,
            Completed = task.Completed,
        }).ToList();

        existing.Tasks = replacementTasks;

        await _dbContext.SaveChangesAsync();
        return existing;
    }
}
