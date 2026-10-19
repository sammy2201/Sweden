using System.ComponentModel.DataAnnotations;

namespace SwedenStart;

public class RoadmapRequestDto
{
    [Required, StringLength(50)]
    public string Origin { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string ResidencePermit { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LiveInSweden { get; set; } = string.Empty;

    [Required, StringLength(12, MinimumLength = 12)]
    public string Personnummer { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string AppliedPersonnummer { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string IdCard { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string BankAccount { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string BankId { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Housing { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string PlanToDrive { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string DrivingLicenceType { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Purpose { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Insurance { get; set; } = string.Empty;
}

public class RoadmapResponseDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string ResidencePermit { get; set; } = string.Empty;
    public string LiveInSweden { get; set; } = string.Empty;
    public string Personnummer { get; set; } = string.Empty;
    public string AppliedPersonnummer { get; set; } = string.Empty;
    public string IdCard { get; set; } = string.Empty;
    public string BankAccount { get; set; } = string.Empty;
    public string BankId { get; set; } = string.Empty;
    public string Housing { get; set; } = string.Empty;
    public string PlanToDrive { get; set; } = string.Empty;
    public string DrivingLicenceType { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Insurance { get; set; } = string.Empty;
    public List<RoadmapTaskDto> Tasks { get; set; } = [];
}

public class RoadmapTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public string RuleId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string State { get; set; } = "Recommended";
    public string StateBeforeCompletion { get; set; } = "Recommended";
    public string? ResourceUrl { get; set; }
    public string RuleExplanation { get; set; } = string.Empty;
    public List<RoadmapTaskPrerequisite> Prerequisites { get; set; } = [];
}

public class UpdateTaskCompletionDto
{
    [Required]
    public bool? Completed { get; set; }
}
