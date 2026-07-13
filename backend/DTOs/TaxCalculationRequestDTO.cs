using System.ComponentModel.DataAnnotations;
namespace SwedenStart;

public class TaxCalculationRequest
{
     [Required]
     [Range(1, 100000000)]
     public decimal MonthlySalary { get; set; }
     [Required]
     public string Municipality { get; set; } = string.Empty;
     [Required]
     [Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
     public int Age { get; set; }
     public bool ChurchMember { get; set; }
}