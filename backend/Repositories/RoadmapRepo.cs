using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace SwedenStart;

public class RoadmapRepository : IRoadmapRepository
{
    private const string PublicTransportCardRuleId = "public-transport-card";
    private const string PublicTransportCardTitle = "Get a Public Transport Card";
    private const string PublicTransportCardDescription = "Purchase a regional travel card for the area where you live or travel. No other roadmap steps are required first.";
    private const string PublicTransportCardExplanation = "A regional travel card can be purchased independently of the respondent's other roadmap answers.";

    private readonly AppDbContext _dbContext;
    private readonly ILogger<RoadmapRepository> _logger;


    public RoadmapRepository(AppDbContext dbContext, ILogger<RoadmapRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Roadmap> SaveAsync(Roadmap roadmap)
    {
        var existing = await _dbContext.Roadmaps
            .Include(r => r.Tasks)
            .FirstOrDefaultAsync(r => r.UserId == roadmap.UserId);

        if (existing is not null)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Roadmap conflict",
                "A roadmap already exists for this user. Cannot create new. Go to profile and update.",
                "ROADMAP_ALREADY_EXISTS");
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
        var roadmap = await _dbContext.Roadmaps
            .Include(r => r.Tasks)
            .FirstOrDefaultAsync(r => r.UserId == userId);

        if (roadmap is not null && EnsurePublicTransportCard(roadmap))
        {
            await _dbContext.SaveChangesAsync();
        }

        return roadmap;
    }

    private bool EnsurePublicTransportCard(Roadmap roadmap)
    {
        var matchingTasks = roadmap.Tasks
            .Where(task => task.RuleId == PublicTransportCardRuleId
                || string.Equals(task.Title, PublicTransportCardTitle, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var transportCard = matchingTasks.FirstOrDefault(task => task.RuleId == PublicTransportCardRuleId)
            ?? matchingTasks.FirstOrDefault();
        var changed = false;

        if (transportCard is null)
        {
            transportCard = new RoadmapTask
            {
                Id = Guid.NewGuid(),
                RoadmapId = roadmap.Id,
                RuleId = PublicTransportCardRuleId,
            };
            _dbContext.RoadmapTasks.Add(transportCard);
            roadmap.Tasks.Add(transportCard);
            changed = true;
        }

        foreach (var duplicate in matchingTasks.Where(task => task.Id != transportCard.Id))
        {
            duplicate.State = "NotApplicable";
            changed = true;
        }

        var completed = transportCard.Completed || transportCard.State == "Completed";
        var desiredState = completed ? "Completed" : "Applicable";
        changed |= transportCard.RuleId != PublicTransportCardRuleId
            || transportCard.Title != PublicTransportCardTitle
            || transportCard.Description != PublicTransportCardDescription
            || transportCard.Category != "Transport"
            || transportCard.Priority != 4
            || transportCard.Completed != completed
            || transportCard.State != desiredState
            || transportCard.StateBeforeCompletion != "Applicable"
            || transportCard.ResourceUrl is not null
            || transportCard.RuleExplanation != PublicTransportCardExplanation
            || transportCard.PrerequisitesJson != "[]";

        transportCard.RuleId = PublicTransportCardRuleId;
        transportCard.Title = PublicTransportCardTitle;
        transportCard.Description = PublicTransportCardDescription;
        transportCard.Category = "Transport";
        transportCard.Priority = 4;
        transportCard.Completed = completed;
        transportCard.State = desiredState;
        transportCard.StateBeforeCompletion = "Applicable";
        transportCard.ResourceUrl = null;
        transportCard.RuleExplanation = PublicTransportCardExplanation;
        transportCard.PrerequisitesJson = "[]";

        return changed;
    }

    public async Task<Roadmap> UpdateAsync(Roadmap roadmap)
    {
        var existing = await _dbContext.Roadmaps
            .Include(r => r.Tasks)
            .FirstOrDefaultAsync(r => r.UserId == roadmap.UserId);

        if (existing is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "Roadmap not found",
                "Roadmap not found for this user.",
                "ROADMAP_NOT_FOUND");
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

        var newTasks = RoadmapTaskReconciler.Reconcile(existing.Tasks, roadmap.Tasks, existing.Id);
        _dbContext.RoadmapTasks.AddRange(newTasks);

        await SaveChangesOrThrowConflictAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        var existing = await _dbContext.Roadmaps
            .FirstOrDefaultAsync(r => r.UserId == userId);

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

    public async Task<Roadmap?> SetTaskCompletionAsync(Guid userId, Guid taskId, bool completed)
    {
        var roadmap = await _dbContext.Roadmaps
            .Include(r => r.Tasks)
            .FirstOrDefaultAsync(r => r.UserId == userId);

        if (roadmap is null)
        {
            return null;
        }

        var task = roadmap.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null || task.State == "NotApplicable")
        {
            return null;
        }

        if (completed)
        {
            if (task.State == "Waiting")
            {
                throw new ApiException(
                    StatusCodes.Status409Conflict,
                    "Step prerequisites are incomplete",
                    "This step is waiting for an update and cannot be marked complete yet.",
                    "ROADMAP_PREREQUISITES_INCOMPLETE");
            }

            var blockingPrerequisites = RoadmapTaskPrerequisiteGuard.GetUnmetTitles(task, roadmap.Tasks);

            if (blockingPrerequisites.Count > 0)
            {
                throw new ApiException(
                    StatusCodes.Status409Conflict,
                    "Step prerequisites are incomplete",
                    $"Complete {string.Join(", ", blockingPrerequisites)} before marking this step complete.",
                    "ROADMAP_PREREQUISITES_INCOMPLETE");
            }
        }
        else
        {
            var completedDependents = RoadmapTaskPrerequisiteGuard.GetCompletedDependentTitles(task, roadmap.Tasks);
            if (completedDependents.Count > 0)
            {
                throw new ApiException(
                    StatusCodes.Status409Conflict,
                    "Step prerequisites are incomplete",
                    $"First mark {string.Join(", ", completedDependents)} incomplete, then update this prerequisite step.",
                    "ROADMAP_PREREQUISITES_INCOMPLETE");
            }
        }

        task.Completed = completed;
        if (completed)
        {
            if (task.State != "Completed") task.StateBeforeCompletion = task.State;
            task.State = "Completed";
        }
        else if (task.State == "Completed")
        {
            task.State = task.StateBeforeCompletion;
        }
        await _dbContext.SaveChangesAsync();
        return roadmap;
    }

    private async Task SaveChangesOrThrowConflictAsync()
    {
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUserRoadmapUniqueViolation(ex))
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Roadmap conflict",
                "A roadmap already exists for this user. Cannot create new. Go to profile and update.",
                "ROADMAP_ALREADY_EXISTS");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entityTypes = string.Join(", ", ex.Entries.Select(entry =>
            {
                var ruleId = entry.Metadata.FindProperty("RuleId") is { } ruleIdProperty
                    ? entry.Property(ruleIdProperty.Name).CurrentValue?.ToString()
                    : null;
                return ruleId is null
                    ? $"{entry.Metadata.ClrType.Name}:{entry.State}"
                    : $"{entry.Metadata.ClrType.Name}:{entry.State}:RuleId={ruleId}";
            }));
            _logger.LogWarning(
                ex,
                "Roadmap persistence encountered a concurrency conflict. Affected entity types/states: {EntityTypes}",
                entityTypes);

            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Roadmap update conflict",
                "Another request changed this roadmap while it was being saved. Reload the roadmap and try again.",
                "ROADMAP_CONCURRENCY_CONFLICT",
                ex);
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
