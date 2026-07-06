namespace SwedenStart.Health;

public class RoadmapService : IRoadmapService
{
    private readonly IRoadmapRepository _roadmapRepository;

    public RoadmapService(IRoadmapRepository roadmapRepository)
        => _roadmapRepository = roadmapRepository;

    public async Task<RoadmapResponseDto> GenerateRoadmapAsync(
        RoadmapRequestDto request)
    {
        var roadmap = new Roadmap
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
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

        return new RoadmapResponseDto
        {
            Id = savedRoadmap.Id,
            UserId = savedRoadmap.UserId,
            CreatedAt = savedRoadmap.CreatedAt,
            Origin = savedRoadmap.Origin,
            ResidencePermit = savedRoadmap.ResidencePermit,
            LiveInSweden = savedRoadmap.LiveInSweden,
            Personnummer = savedRoadmap.Personnummer,
            AppliedPersonnummer = savedRoadmap.AppliedPersonnummer,
            IdCard = savedRoadmap.IdCard,
            BankAccount = savedRoadmap.BankAccount,
            BankId = savedRoadmap.BankId,
            Housing = savedRoadmap.Housing,
            PlanToDrive = savedRoadmap.PlanToDrive,
            DrivingLicenceType = savedRoadmap.DrivingLicenceType,
            Purpose = savedRoadmap.Purpose,
            Insurance = savedRoadmap.Insurance,
            Tasks = savedRoadmap.Tasks.Select(task => new RoadmapTaskDto
            {
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

        var liveInSweden = string.Equals(
            request.LiveInSweden,
            "Yes",
            StringComparison.OrdinalIgnoreCase);

        if (liveInSweden)
        {
            if (string.Equals(
                    request.Personnummer,
                    "No",
                    StringComparison.OrdinalIgnoreCase))
            {
                tasks.Add(new RoadmapTask
                {
                    Title = "Apply for Personnummer",
                    Description = "Book an appointment with Skatteverket.",
                    Completed = false,
                });
            }

            if (string.Equals(
                    request.Personnummer,
                    "Yes",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(
                        request.IdCard,
                        "No",
                        StringComparison.OrdinalIgnoreCase))
                {
                    tasks.Add(new RoadmapTask
                    {
                        Title = "Apply for Swedish ID Card",
                        Description = "Order your ID card after receiving your personnummer.",
                        Completed = false,
                    });
                }

                if (string.Equals(
                        request.BankAccount,
                        "Yes",
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        request.BankId,
                        "No",
                        StringComparison.OrdinalIgnoreCase))
                {
                    tasks.Add(new RoadmapTask
                    {
                        Title = "Activate BankID",
                        Description = "Set up BankID after opening your bank account.",
                        Completed = false,
                    });
                }

                if (string.Equals(
                        request.Insurance,
                        "No",
                        StringComparison.OrdinalIgnoreCase))
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
                "Looking for work",
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
                "Still looking",
                StringComparison.OrdinalIgnoreCase))
        {
            tasks.Add(new RoadmapTask
            {
                Title = "Join Housing Queues",
                Description = "Register with Boplats, HomeQ and your municipality.",
                Completed = false,
            });
        }

        if (string.Equals(
                request.PlanToDrive,
                "Yes",
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                request.DrivingLicenceType,
                "Other",
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
