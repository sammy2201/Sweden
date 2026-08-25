using Microsoft.EntityFrameworkCore;
using Npgsql;
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

        if (existing is not null)
        {
            throw new RoadmapConflictException(
                "A roadmap already exists for this user. Cannot create new. Go to profile and update.");
        }

        roadmap.Id = roadmap.Id == Guid.Empty ? Guid.NewGuid() : roadmap.Id;
        roadmap.CreatedAt = roadmap.CreatedAt == default ? DateTime.UtcNow : roadmap.CreatedAt;

        foreach (var task in roadmap.Tasks)
        {
            task.Id = task.Id == Guid.Empty ? Guid.NewGuid() : task.Id;
        }

        await _dbContext.Roadmaps.AddAsync(roadmap);
        await SaveChangesOrThrowConflictAsync();
        return roadmap;
    }


    public async Task<Roadmap?> GetByUserIdAsync(Guid userId)
    {
        return await _dbContext.Roadmaps
            .Include(r => r.Tasks)
            .FirstOrDefaultAsync(r => r.UserId == userId);
    }

    public async Task<Roadmap> UpdateAsync(Roadmap roadmap)
    {
        var existing = await _dbContext.Roadmaps
            .FirstOrDefaultAsync(r => r.UserId == roadmap.UserId);

        if (existing is null)
        {
            throw new RoadmapNotFoundException("Roadmap not found for this user.");
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

        await _dbContext.RoadmapTasks
            .Where(task => task.RoadmapId == existing.Id)
            .ExecuteDeleteAsync();

        var replacementTasks = roadmap.Tasks
            .Select(task => new RoadmapTask
            {
                Id = task.Id == Guid.Empty ? Guid.NewGuid() : task.Id,
                RoadmapId = existing.Id,
                Title = task.Title,
                Description = task.Description,
                Completed = task.Completed,
            })
            .ToList();

        _dbContext.RoadmapTasks.AddRange(replacementTasks);
        existing.Tasks = replacementTasks;

        await SaveChangesOrThrowConflictAsync();
        return existing;
    }

    private async Task SaveChangesOrThrowConflictAsync()
    {
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUserRoadmapUniqueViolation(ex))
        {
            throw new RoadmapConflictException(
                "A roadmap already exists for this user. Cannot create new. Go to profile and update.");
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new RoadmapNotFoundException(
                "Roadmap could not be updated because it was changed or deleted. Please refresh and try again.");
        }
    }

    private static bool IsUserRoadmapUniqueViolation(DbUpdateException ex)
    {
        if (ex.InnerException is not PostgresException pgEx)
        {
            return false;
        }

        return pgEx.SqlState == PostgresErrorCodes.UniqueViolation
            && string.Equals(pgEx.ConstraintName, "IX_Roadmaps_UserId", StringComparison.Ordinal);
    }

}
