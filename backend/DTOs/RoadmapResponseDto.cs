namespace SwedenStart;

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
