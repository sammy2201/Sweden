using System.Globalization;
using System.Text;

namespace SwedenStart;

public class CountyService
{
     private readonly CountyDataProvider _countyDataProvider;

     public CountyService(CountyDataProvider countyDataProvider)
     {
          _countyDataProvider = countyDataProvider;
     }

     public IEnumerable<CountyDto> GetCounties() => _countyDataProvider.Counties.Select(county => new CountyDto
     {
          Id = county.Id,
          Name = county.Name,
          Description = county.Description,
          TransportProviderName = county.TransportProviderName,
          OfficialTicketUrl = county.OfficialTicketUrl,
          JourneyPlannerUrl = county.JourneyPlannerUrl,
     });

     public bool IsValidCountyId(string countyId) =>
         _countyDataProvider.Counties.Any(county =>
             string.Equals(county.Id, countyId, StringComparison.Ordinal));

     public string ResolveCountyId(string? value)
     {
          var key = Normalize(value);
          if (string.IsNullOrWhiteSpace(key))
               return string.Empty;

          return _countyDataProvider.Counties.FirstOrDefault(county =>
              Normalize(county.Id) == key || county.Aliases.Any(alias => Normalize(alias) == key))?.Id ?? string.Empty;
     }

     public string[] GetCountyFilterValues(string? value)
     {
          var countyId = ResolveCountyId(value);
          if (string.IsNullOrWhiteSpace(countyId))
               return [value?.Trim() ?? string.Empty];

          var county = _countyDataProvider.Counties.First(entry => entry.Id == countyId);
          return county.Aliases
              .SelectMany(alias => new[] { alias, $"{alias} län" })
              .Append(county.Id)
              .Append(county.Name)
              .Distinct(StringComparer.OrdinalIgnoreCase)
              .ToArray();
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
}