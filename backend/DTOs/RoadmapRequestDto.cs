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

    [Required, StringLength(50)]
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
