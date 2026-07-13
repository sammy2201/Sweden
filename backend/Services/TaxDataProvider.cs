using System.Text.Json;
using System.Text.Json.Serialization;

namespace SwedenStart;

public class TaxDataProvider
{
     public IReadOnlyList<MunicipalityTaxRate> TaxRates { get; }

     public TaxDataProvider(IWebHostEnvironment env)
     {
          var path = Path.Combine(
              env.ContentRootPath,
              "Data",
              "skattesatser-kommuner-2026.json");

          var json = File.ReadAllText(path);

          var options = new JsonSerializerOptions
          {
               PropertyNameCaseInsensitive = true,
               NumberHandling = JsonNumberHandling.AllowReadingFromString
          };

          TaxRates = JsonSerializer.Deserialize<List<MunicipalityTaxRate>>(json, options)!
                ?? throw new InvalidOperationException("Unable to load tax data.");
     }
}