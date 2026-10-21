namespace SwedenStart;

public class RoadmapService : IRoadmapService
{
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly RoadmapTaskGenerator _taskGenerator;

    public RoadmapService(IRoadmapRepository roadmapRepository, RoadmapTaskGenerator taskGenerator)
    { _roadmapRepository = roadmapRepository; _taskGenerator = taskGenerator; }

    public async Task<RoadmapResponseDto> GenerateRoadmapServiceAsync(
        RoadmapRequestDto request, Guid userId)
    {
        var roadmap = new Roadmap
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
        };
        ApplyAnswers(roadmap, request);
        roadmap.Tasks = _taskGenerator.Generate(request);
        foreach (var task in roadmap.Tasks)
            task.Id = Guid.NewGuid();

        var savedRoadmap = await _roadmapRepository.SaveRoadmapRepoAsync(roadmap);

        return MapToResponse(savedRoadmap);
    }

    public async Task<RoadmapResponseDto?> GetRoadmapServiceAsync(Guid userId)
    {
        var roadmap = await _roadmapRepository.GetRoadmapByUserIdRepoAsync(userId);
        if (roadmap is null)
            return null;

        await ReconcileSavedRoadmapAsync(roadmap);
        return MapToResponse(roadmap);
    }

    public async Task<RoadmapResponseDto> UpdateRoadmapServiceAsync(RoadmapRequestDto request, Guid userId)
    {
        var roadmap = await _roadmapRepository.GetRoadmapByUserIdRepoAsync(userId);
        if (roadmap is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "Roadmap not found",
                "Roadmap not found for this user.");
        }

        ApplyAnswers(roadmap, request);
        var generatedTasks = _taskGenerator.Generate(request);
        var newTasks = RoadmapTaskReconciler.Reconcile(roadmap.Tasks, generatedTasks, roadmap.Id);
        roadmap.Tasks.AddRange(newTasks);

        await _roadmapRepository.SaveRoadmapChangesRepoAsync();
        return MapToResponse(roadmap);

    }

    public async Task<bool> DeleteRoadmapServiceAsync(Guid userId)
        => await _roadmapRepository.DeleteRoadmapRepoAsync(userId);

    public async Task<RoadmapResponseDto> SetTaskCompletionServiceAsync(Guid userId, Guid taskId, bool completed)
    {
        var roadmap = await _roadmapRepository.GetRoadmapByUserIdRepoAsync(userId);
        if (roadmap is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "Roadmap not found",
                "No roadmap exists for this user.");
        }

        await ReconcileSavedRoadmapAsync(roadmap);

        var task = roadmap.Tasks.FirstOrDefault(roadmapTask => roadmapTask.Id == taskId);
        if (task is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "Roadmap task not found",
                "This task is no longer part of your roadmap. Refresh the roadmap and try again.");
        }

        if (task.State == "NotApplicable")
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Roadmap task is no longer applicable",
                "This task no longer applies to your answers. Refresh the roadmap to see the current steps.");
        }

        if (completed)
        {
            if (task.State == "Waiting")
            {
                throw new ApiException(
                    StatusCodes.Status409Conflict,
                    "Task is waiting",
                    "This step is waiting for an update and cannot be marked complete yet.");
            }
        }

        if (completed)
        {
            if (task.State != "Completed") task.StateBeforeCompletion = task.State;
            task.State = "Completed";
        }
        else if (task.State == "Completed")
        {
            task.State = task.StateBeforeCompletion;
        }
        task.Completed = completed;
        await _roadmapRepository.SaveRoadmapChangesRepoAsync();

        return MapToResponse(roadmap);
    }

    private async Task ReconcileSavedRoadmapAsync(Roadmap roadmap)
    {
        RoadmapTaskReconciler.ReconcileExistingRoadmap(roadmap, _taskGenerator);
        await _roadmapRepository.SaveRoadmapChangesRepoAsync();
    }

    private static void ApplyAnswers(Roadmap roadmap, RoadmapRequestDto request)
    {
        roadmap.Origin = request.Origin;
        roadmap.ResidencePermit = request.ResidencePermit;
        roadmap.LiveInSweden = request.LiveInSweden;
        roadmap.Personnummer = request.Personnummer;
        roadmap.AppliedPersonnummer = request.AppliedPersonnummer;
        roadmap.IdCard = request.IdCard;
        roadmap.BankAccount = request.BankAccount;
        roadmap.BankId = request.BankId;
        roadmap.Housing = request.Housing;
        roadmap.PlanToDrive = request.PlanToDrive;
        roadmap.DrivingLicenceType = request.DrivingLicenceType;
        roadmap.Purpose = request.Purpose;
        roadmap.Insurance = request.Insurance;
    }

    private static RoadmapResponseDto MapToResponse(Roadmap roadmap)
    {
        return new RoadmapResponseDto
        {
            Id = roadmap.Id,
            UserId = roadmap.UserId.ToString(),
            Origin = roadmap.Origin,
            ResidencePermit = roadmap.ResidencePermit,
            LiveInSweden = roadmap.LiveInSweden,
            Personnummer = roadmap.Personnummer,
            AppliedPersonnummer = roadmap.AppliedPersonnummer,
            IdCard = roadmap.IdCard,
            BankAccount = roadmap.BankAccount,
            BankId = roadmap.BankId,
            Housing = roadmap.Housing,
            PlanToDrive = roadmap.PlanToDrive,
            DrivingLicenceType = roadmap.DrivingLicenceType,
            Purpose = roadmap.Purpose,
            Insurance = roadmap.Insurance,
            Tasks = RoadmapTaskOrdering.ByPriority(roadmap.Tasks).Select(task => new RoadmapTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Completed = task.Completed,
                RuleId = task.RuleId,
                Category = task.Category,
                Priority = task.Priority,
                State = task.State,
                StateBeforeCompletion = task.StateBeforeCompletion,
                ResourceUrl = task.ResourceUrl,
                RuleExplanation = task.RuleExplanation,
            }).ToList(),
        };
    }
}
