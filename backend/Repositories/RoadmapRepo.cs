using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace SwedenStart;

public class RoadmapRepository : IRoadmapRepository
{
    private const string PublicTransportCardRuleId = "public-transport-card";
    private const string PublicTransportCardTitle = "Get a Public Transport Card";
    private const string PublicTransportCardDescription = "Optional: check whether a regional travel card is useful for the area where you live or travel. You can do this independently of other roadmap steps.";
    private const string PublicTransportCardExplanation = "A regional travel card may be useful depending on the respondent's location and travel habits; it is optional and independent of other roadmap steps.";

    private readonly AppDbContext _dbContext;
    private readonly ILogger<RoadmapRepository> _logger;
    private readonly RoadmapTaskGenerator _taskGenerator;


    public RoadmapRepository(
        AppDbContext dbContext,
        ILogger<RoadmapRepository> logger,
        RoadmapTaskGenerator taskGenerator)
    {
        _dbContext = dbContext;
        _logger = logger;
        _taskGenerator = taskGenerator;
    }

    public async Task<Roadmap> SaveAsync(Roadmap roadmap)
    {
        if (await _dbContext.Roadmaps.AnyAsync(existingRoadmap => existingRoadmap.UserId == roadmap.UserId))
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
            .Include(roadmap => roadmap.Tasks)
            .FirstOrDefaultAsync(roadmap => roadmap.UserId == userId);

        if (roadmap is not null)
        {
            await ReconcileSavedRoadmapAsync(roadmap);
        }

        return roadmap;
    }

    private void EnsurePublicTransportCard(Roadmap roadmap)
    {
        var matchingTasks = roadmap.Tasks
            .Where(task => task.RuleId == PublicTransportCardRuleId
                || string.Equals(task.Title, PublicTransportCardTitle, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var transportCard = matchingTasks.FirstOrDefault(task => task.RuleId == PublicTransportCardRuleId)
            ?? matchingTasks.FirstOrDefault();
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
        }

        foreach (var duplicate in matchingTasks.Where(task => task.Id != transportCard.Id))
        {
            duplicate.State = "NotApplicable";
        }

        var completed = transportCard.Completed || transportCard.State == "Completed";
        var desiredState = completed ? "Completed" : "Optional";

        transportCard.RuleId = PublicTransportCardRuleId;
        transportCard.Title = PublicTransportCardTitle;
        transportCard.Description = PublicTransportCardDescription;
        transportCard.Category = "Transport";
        transportCard.Priority = 4;
        transportCard.Completed = completed;
        transportCard.State = desiredState;
        transportCard.StateBeforeCompletion = "Optional";
        transportCard.ResourceUrl = null;
        transportCard.RuleExplanation = PublicTransportCardExplanation;
        transportCard.PrerequisitesJson = "[]";
    }

    public async Task<Roadmap> UpdateAsync(Roadmap roadmap)
    {
        var existing = await _dbContext.Roadmaps
            .Include(existingRoadmap => existingRoadmap.Tasks)
            .FirstOrDefaultAsync(existingRoadmap => existingRoadmap.UserId == roadmap.UserId);

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

    public async Task<Roadmap> SetTaskCompletionAsync(Guid userId, Guid taskId, bool completed)
    {
        var roadmap = await _dbContext.Roadmaps
            .Include(roadmap => roadmap.Tasks)
            .FirstOrDefaultAsync(roadmap => roadmap.UserId == userId);

        if (roadmap is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "Roadmap not found",
                "No roadmap exists for this user.",
                "ROADMAP_NOT_FOUND");
        }

        await ReconcileSavedRoadmapAsync(roadmap);

        var task = roadmap.Tasks.FirstOrDefault(roadmapTask => roadmapTask.Id == taskId);
        if (task is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "Roadmap task not found",
                "This task is no longer part of your roadmap. Refresh the roadmap and try again.",
                "ROADMAP_TASK_NOT_FOUND");
        }

        if (task.State == "NotApplicable")
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Roadmap task is no longer applicable",
                "This task no longer applies to your answers. Refresh the roadmap to see the current steps.",
                "ROADMAP_TASK_NOT_APPLICABLE");
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

        RoadmapTaskPrerequisiteGuard.ApplyCompletionState(task, completed);
        await _dbContext.SaveChangesAsync();
        return roadmap;
    }

    private async Task ReconcileSavedRoadmapAsync(Roadmap roadmap)
    {
        RoadmapTaskReconciler.ReconcileExistingRoadmap(roadmap, _taskGenerator);
        EnsurePublicTransportCard(roadmap);
        if (_dbContext.ChangeTracker.HasChanges())
            await _dbContext.SaveChangesAsync();
    }

    private async Task SaveChangesOrThrowConflictAsync()
    {
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException databaseUpdateException) when (IsUserRoadmapUniqueViolation(databaseUpdateException))
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Roadmap conflict",
                "A roadmap already exists for this user. Cannot create new. Go to profile and update.",
                "ROADMAP_ALREADY_EXISTS");
        }
        catch (DbUpdateConcurrencyException exception)
        {
            var entityTypes = string.Join(", ", exception.Entries.Select(entry =>
            {
                var ruleId = entry.Metadata.FindProperty("RuleId") is { } ruleIdProperty
                    ? entry.Property(ruleIdProperty.Name).CurrentValue?.ToString()
                    : null;
                return ruleId is null
                    ? $"{entry.Metadata.ClrType.Name}:{entry.State}"
                    : $"{entry.Metadata.ClrType.Name}:{entry.State}:RuleId={ruleId}";
            }));
            _logger.LogWarning(
                exception,
                "Roadmap persistence encountered a concurrency conflict. Affected entity types/states: {EntityTypes}",
                entityTypes);

            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Roadmap update conflict",
                "Another request changed this roadmap while it was being saved. Reload the roadmap and try again.",
                "ROADMAP_CONCURRENCY_CONFLICT",
                exception);
        }
    }

    private static bool IsUserRoadmapUniqueViolation(DbUpdateException databaseUpdateException)
    {
        if (databaseUpdateException.InnerException is not PostgresException postgresException)
        {
            return false;
        }

        return postgresException.SqlState == PostgresErrorCodes.UniqueViolation
            && string.Equals(postgresException.ConstraintName, "IX_Roadmaps_UserId", StringComparison.Ordinal);
    }

}
