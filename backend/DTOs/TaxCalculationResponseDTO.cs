namespace SwedenStart;

public class TaxCalculationResponse
{
     public decimal GrossSalary { get; set; }

     public decimal MunicipalTax { get; set; }

     public decimal StateTax { get; set; }

     public decimal ChurchFee { get; set; }

     public decimal TaxCredits { get; set; }

     public decimal TotalTax { get; set; }

     public decimal TaxAmount { get; set; }

     public decimal NetSalary { get; set; }

     public decimal EffectiveTaxRate { get; set; }

     public decimal TaxRate { get; set; }

     public string Municipality { get; set; } = string.Empty;

     public int TaxTable { get; set; }
}