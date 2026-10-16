namespace SwedenStart;

public class RoadmapService : IRoadmapService
{
    private readonly IRoadmapRepository _roadmapRepository;

    public RoadmapService(IRoadmapRepository roadmapRepository)
        => _roadmapRepository = roadmapRepository;

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
            Tasks = BuildTasks(request),
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
            Tasks = BuildTasks(request),
        };

        var updatedRoadmap = await _roadmapRepository.UpdateAsync(roadmap);
        return MapToResponse(updatedRoadmap);

    }

    public async Task<bool> DeleteRoadmapAsync(Guid userId)
        => await _roadmapRepository.DeleteAsync(userId);

    public async Task<RoadmapResponseDto> SetTaskCompletionAsync(Guid userId, Guid taskId, bool completed)
    {
        var roadmap = await _roadmapRepository.SetTaskCompletionAsync(userId, taskId, completed)
            ?? throw new RoadmapNotFoundException("Roadmap or task not found for this user.");

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
            Tasks = roadmap.Tasks.Select(task => new RoadmapTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Completed = task.Completed,
            }).ToList(),
        };
    }
    private static List<RoadmapTask> BuildTasks(RoadmapRequestDto request)
    {
        var tasks = new List<RoadmapTask>
        {
            new()
            {
                Title = "Register your address",
                Description = "Register your address with Skatteverket.",
                Completed = true,
            },
        };

        var liveInSweden = RoadmapValues.IsYes(request.LiveInSweden);

        if (liveInSweden)
        {
            if (RoadmapValues.IsNo(request.Personnummer))
            {
                tasks.Add(new RoadmapTask
                {
                    Title = "Apply for Personnummer",
                    Description = "Book an appointment with Skatteverket.",
                    Completed = false,
                });
            }

            if (RoadmapValues.IsYes(request.Personnummer))
            {
                if (RoadmapValues.IsNo(request.IdCard))
                {
                    tasks.Add(new RoadmapTask
                    {
                        Title = "Apply for Swedish ID Card",
                        Description = "Order your ID card after receiving your personnummer.",
                        Completed = false,
                    });
                }

                if (RoadmapValues.IsYes(request.BankAccount) &&
                    RoadmapValues.IsNo(request.BankId))
                {
                    tasks.Add(new RoadmapTask
                    {
                        Title = "Activate BankID",
                        Description = "Set up BankID after opening your bank account.",
                        Completed = false,
                    });
                }

                if (RoadmapValues.IsNo(request.Insurance))
                {
                    tasks.Add(new RoadmapTask
                    {
                        Title = "Register with Försäkringskassan",
                        Description = "Apply for Swedish social insurance.",
                        Completed = false,
                    });
                }
            }
        }

        if (string.Equals(
                request.Purpose,
                RoadmapValues.LookingForWork,
                StringComparison.OrdinalIgnoreCase))
        {
            tasks.Add(new RoadmapTask
            {
                Title = "Register with Arbetsförmedlingen",
                Description = "Create a profile and search for jobs.",
                Completed = false,
            });
        }

        if (string.Equals(
                request.Housing,
                RoadmapValues.StillLooking,
                StringComparison.OrdinalIgnoreCase))
        {
            tasks.Add(new RoadmapTask
            {
                Title = "Join Housing Queues",
                Description = "Register with Boplats, HomeQ and your municipality.",
                Completed = false,
            });
        }

        if (RoadmapValues.IsYes(request.PlanToDrive) &&
            string.Equals(
                request.DrivingLicenceType,
                RoadmapValues.Other,
                StringComparison.OrdinalIgnoreCase))
        {
            tasks.Add(new RoadmapTask
            {
                Title = "Check Driving Licence Rules",
                Description = "Find out whether your licence can be exchanged.",
                Completed = false,
            });
        }

        tasks.Add(new RoadmapTask
        {
            Title = "Get a Public Transport Card",
            Description = "Purchase a regional travel card.",
            Completed = false,
        });

        return tasks;
    }
}
