namespace SwedenStart;

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
