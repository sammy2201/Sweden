namespace SwedenStart;

public class TaxCalculationRequest
{
     public decimal MonthlySalary { get; set; }

     public string Municipality { get; set; } = string.Empty;

     public int Age { get; set; }

     public bool ChurchMember { get; set; }
}