namespace SwedenStart;

public class RoadmapService : IRoadmapService
{
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly RoadmapTaskGenerator _taskGenerator;

    public RoadmapService(IRoadmapRepository roadmapRepository, RoadmapTaskGenerator taskGenerator)
    { _roadmapRepository = roadmapRepository; _taskGenerator = taskGenerator; }

    public async Task<RoadmapResponseDto> GenerateRoadmapAsync(
        RoadmapRequestDto request, Guid userId)
    {

        var roadmap = new Roadmap
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Origin = request.Origin,
            ResidencePermit = request.ResidencePermit,
            LiveInSweden = request.LiveInSweden,
            Personnummer = request.Personnummer,
            AppliedPersonnummer = request.AppliedPersonnummer,
            IdCard = request.IdCard,
            BankAccount = request.BankAccount,
            BankId = request.BankId,
            Housing = request.Housing,
            PlanToDrive = request.PlanToDrive,
            DrivingLicenceType = request.DrivingLicenceType,
            Purpose = request.Purpose,
            Insurance = request.Insurance,
            Tasks = _taskGenerator.Generate(request),
        };

        var savedRoadmap = await _roadmapRepository.SaveAsync(roadmap);

        return MapToResponse(savedRoadmap);
    }

    public async Task<RoadmapResponseDto?> GetRoadmapAsync(Guid userId)
    {
        var roadmap = await _roadmapRepository.GetByUserIdAsync(userId);

        return roadmap == null ? null : MapToResponse(roadmap);
    }

    public async Task<RoadmapResponseDto> UpdateRoadmapAsync(RoadmapRequestDto request, Guid userId)
    {
        var roadmap = new Roadmap
        {
            UserId = userId,
            Origin = request.Origin,
            ResidencePermit = request.ResidencePermit,
            LiveInSweden = request.LiveInSweden,
            Personnummer = request.Personnummer,
            AppliedPersonnummer = request.AppliedPersonnummer,
            IdCard = request.IdCard,
            BankAccount = request.BankAccount,
            BankId = request.BankId,
            Housing = request.Housing,
            PlanToDrive = request.PlanToDrive,
            DrivingLicenceType = request.DrivingLicenceType,
            Purpose = request.Purpose,
            Insurance = request.Insurance,
            Tasks = _taskGenerator.Generate(request),
        };

        var updatedRoadmap = await _roadmapRepository.UpdateAsync(roadmap);
        return MapToResponse(updatedRoadmap);

    }

    public async Task<bool> DeleteRoadmapAsync(Guid userId)
        => await _roadmapRepository.DeleteAsync(userId);

    public async Task<RoadmapResponseDto> SetTaskCompletionAsync(Guid userId, Guid taskId, bool completed)
    {
        var roadmap = await _roadmapRepository.SetTaskCompletionAsync(userId, taskId, completed)
            ?? throw new ApiException(
                StatusCodes.Status404NotFound,
                "Roadmap not found",
                "Roadmap or task not found for this user.",
                "ROADMAP_NOT_FOUND");

        return MapToResponse(roadmap);
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
            Tasks = RoadmapTaskOrdering.Topological(roadmap.Tasks).Select(task => new RoadmapTaskDto
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
                Prerequisites = task.Prerequisites,
            }).ToList(),
        };
    }
}
