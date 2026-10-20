using System.Globalization;
using System.Text;
using System.Text.Json;

namespace SwedenStart;

public static class CountyData
{
     private static readonly List<CountyEntry> Entries = Load();
     private static readonly List<CountyDto> Counties = Entries.Select(entry => entry.ToDto()).ToList();

     public static IEnumerable<CountyDto> GetCounties() => Counties;

     public static bool IsValidCountyId(string countyId) =>
         Counties.Any(county => string.Equals(county.Id, countyId, StringComparison.Ordinal));

     public static string ResolveCountyId(string? value)
     {
          var key = Normalize(value);
          if (string.IsNullOrWhiteSpace(key))
               return string.Empty;

          return Entries.FirstOrDefault(entry =>
              Normalize(entry.Id) == key || entry.Aliases.Any(alias => Normalize(alias) == key))?.Id ?? string.Empty;
     }

     public static string[] GetCountyFilterValues(string? value)
     {
          var countyId = ResolveCountyId(value);
          if (string.IsNullOrWhiteSpace(countyId))
               return [value?.Trim() ?? string.Empty];

          var county = Entries.First(entry => entry.Id == countyId);
          return county.Aliases
              .SelectMany(alias => new[] { alias, $"{alias} län" })
              .Append(county.Id)
              .Append(county.Name)
              .Distinct(StringComparer.OrdinalIgnoreCase)
              .ToArray();
     }

     private static List<CountyEntry> Load()
     {
          var path = Path.Combine(AppContext.BaseDirectory, "Data", "Counties.json");
          var data = JsonSerializer.Deserialize<CountyFile>(File.ReadAllText(path), new JsonSerializerOptions
          {
               PropertyNameCaseInsensitive = true,
          });

          return data?.Counties ?? throw new InvalidDataException("County data is missing from Counties.json.");
     }

     private static string Normalize(string? value)
     {
          if (string.IsNullOrWhiteSpace(value))
               return string.Empty;

          var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
          var normalized = new string(decomposed
              .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
              .Where(char.IsLetterOrDigit)
              .Select(char.ToLowerInvariant)
              .ToArray());

          return normalized.EndsWith("lan", StringComparison.Ordinal)
              ? normalized[..^3]
              : normalized;
     }

     private sealed class CountyFile
     {
          public List<CountyEntry> Counties { get; set; } = [];
     }

     private sealed class CountyEntry
     {
          public string Id { get; set; } = string.Empty;
          public string Name { get; set; } = string.Empty;
          public string Description { get; set; } = string.Empty;
          public string TransportProviderName { get; set; } = string.Empty;
          public string OfficialTicketUrl { get; set; } = string.Empty;
          public string? JourneyPlannerUrl { get; set; }
          public string[] Aliases { get; set; } = [];

          public CountyDto ToDto() => new()
          {
               Id = Id,
               Name = Name,
               Description = Description,
               TransportProviderName = TransportProviderName,
               OfficialTicketUrl = OfficialTicketUrl,
               JourneyPlannerUrl = JourneyPlannerUrl,
          };
     }
}