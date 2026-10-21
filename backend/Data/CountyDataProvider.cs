using System.Text.Json;

namespace SwedenStart;

public class CountyDataProvider
{
     private readonly IReadOnlyList<CountyDefinition> _counties;

     public CountyDataProvider(IWebHostEnvironment environment)
     {
          var path = Path.Combine(environment.ContentRootPath, "Data", "Counties.json");
          var data = JsonSerializer.Deserialize<CountyFile>(File.ReadAllText(path), new JsonSerializerOptions
          {
               PropertyNameCaseInsensitive = true,
          });

          _counties = data?.Counties
              ?? throw new InvalidDataException("County data is missing from Counties.json.");
     }

     public IReadOnlyList<CountyDefinition> Counties => _counties;

     private sealed class CountyFile
     {
          public List<CountyDefinition> Counties { get; set; } = [];
     }
}

public class CountyDefinition
{
     public string Id { get; set; } = string.Empty;
     public string Name { get; set; } = string.Empty;
     public string Description { get; set; } = string.Empty;
     public string TransportProviderName { get; set; } = string.Empty;
     public string OfficialTicketUrl { get; set; } = string.Empty;
     public string? JourneyPlannerUrl { get; set; }
     public string[] Aliases { get; set; } = [];
}