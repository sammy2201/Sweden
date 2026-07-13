namespace SwedenStart;

public interface ITaxService
{
     IEnumerable<MunicipalityTaxRate> GetTaxRates();

     TaxCalculationResponse Calculate(TaxCalculationRequest request);

     IEnumerable<string> GetMunicipalities();
}