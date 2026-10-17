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

public class TaxCalculationResponse
{
     public decimal GrossSalary { get; set; }

     public decimal MunicipalTax { get; set; }

     public decimal StateTax { get; set; }

     public decimal ChurchFee { get; set; }

     public decimal TaxCredits { get; set; }

     public decimal TotalTax { get; set; }

     public decimal NetSalary { get; set; }

     public decimal EffectiveTaxRate { get; set; }

     public string Municipality { get; set; } = string.Empty;

     public int TaxTable { get; set; }
}

public class MunicipalityTaxRate
{
     public int Year { get; set; }

     public string ParishCode { get; set; } = string.Empty;

     public string Municipality { get; set; } = string.Empty;

     public string Parish { get; set; } = string.Empty;

     public decimal TotalTaxIncludingChurch { get; set; }

     public decimal TotalTaxExcludingChurch { get; set; }

     public decimal MunicipalTax { get; set; }

     public decimal RegionalTax { get; set; }

     public decimal BurialFee { get; set; }

     public decimal ChurchFee { get; set; }

     public decimal TaxRate => TotalTaxExcludingChurch;

     public int TaxTable => (int)Math.Round(TotalTaxExcludingChurch, MidpointRounding.AwayFromZero);
}
