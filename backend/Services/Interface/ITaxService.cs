namespace SwedenStart;

public interface ITaxService
{
     IEnumerable<MunicipalityTaxRate> GetTaxRatesService();

     TaxCalculationResponse CalculateService(TaxCalculationRequest request);

     IEnumerable<string> GetMunicipalitiesService();
}