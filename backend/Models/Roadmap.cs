namespace SwedenStart;

public class Roadmap
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
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

    public List<RoadmapTask> Tasks { get; set; } = [];
}
