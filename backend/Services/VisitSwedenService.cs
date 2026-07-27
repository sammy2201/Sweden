using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SwedenStart;

public class VisitSwedenService : IVisitSwedenService
{
     private const int SourcePageSize = 100;

     private readonly HttpClient _httpClient;
     private readonly ILogger<VisitSwedenService> _logger;
     private readonly IVisitSwedenRepository _visitSwedenRepository;
     private readonly ICountyLookupService _countyLookupService;

     public VisitSwedenService(
         HttpClient httpClient,
         ILogger<VisitSwedenService> logger,
         IVisitSwedenRepository visitSwedenRepository,
         ICountyLookupService countyLookupService)
     {
          _httpClient = httpClient;
          _logger = logger;
          _visitSwedenRepository = visitSwedenRepository;
          _countyLookupService = countyLookupService;
     }

     public async Task SyncAttractionsAsync()
     {
          var mapped = new List<Attraction>();
          var skippedMissingImage = 0;
          var skippedMissingCoordinates = 0;
          var skippedTotal = 0;

          for (var sourcePage = 0; ; sourcePage++)
          {
               var offset = sourcePage * SourcePageSize;
               var query = "public:true AND rdfType:http\\://schema.org/*";
               var endpoint =
                    $"search?type=solr&query={Uri.EscapeDataString(query)}&limit={SourcePageSize}&offset={offset}&rdfFormat=application/ld+json";

               using var response = await _httpClient.GetAsync(endpoint);
               if (!response.IsSuccessStatusCode)
               {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Visit Sweden sync failed: {StatusCode} {Body}", (int)response.StatusCode, body);
                    throw new InvalidOperationException("Visit Sweden API request failed.");
               }

               await using var stream = await response.Content.ReadAsStreamAsync();
               using var document = await JsonDocument.ParseAsync(stream);

               var children = GetChildren(document.RootElement);
               if (children.Count == 0)
                    break;

               foreach (var child in children)
               {
                    var attraction = MapAttraction(child);
                    if (attraction is not null)
                    {
                         var missingImage = string.IsNullOrWhiteSpace(attraction.ImageUrl);
                         var missingCoordinates = !HasValidCoordinates(attraction.Latitude, attraction.Longitude);

                         if (missingImage || missingCoordinates)
                         {
                              if (missingImage)
                              {
                                   skippedMissingImage++;
                              }

                              if (missingCoordinates)
                              {
                                   skippedMissingCoordinates++;
                              }

                              skippedTotal++;
                              continue;
                         }

                         mapped.Add(attraction);
                    }
               }
          }

          if (skippedTotal > 0)
          {
               _logger.LogWarning(
                    "Visit Sweden sync skipped records due to missing media/location data. MissingImage: {MissingImageCount}, MissingCoordinates: {MissingCoordinatesCount}, TotalSkipped: {TotalSkippedCount}",
                    skippedMissingImage,
                    skippedMissingCoordinates,
                    skippedTotal);
          }

          await _visitSwedenRepository.UpsertAttractionsAsync(mapped);
     }

     private static bool HasValidCoordinates(double? latitude, double? longitude)
     {
          if (!latitude.HasValue || !longitude.HasValue)
               return false;

          var lat = latitude.Value;
          var lon = longitude.Value;

          if (double.IsNaN(lat) || double.IsInfinity(lat) || double.IsNaN(lon) || double.IsInfinity(lon))
               return false;

          return lat is >= -90 and <= 90 && lon is >= -180 and <= 180;
     }

     private static List<JsonElement> GetChildren(JsonElement root)
     {
          if (!root.TryGetProperty("resource", out var resource) ||
              !resource.TryGetProperty("children", out var children) ||
              children.ValueKind != JsonValueKind.Array)
          {
               return [];
          }

          return children.EnumerateArray().ToList();
     }

     private Attraction? MapAttraction(JsonElement child)
     {
          var contextId = GetString(child, "contextId");
          var entryId = GetString(child, "entryId");
          if (string.IsNullOrWhiteSpace(contextId) || string.IsNullOrWhiteSpace(entryId))
               return null;

          if (!TryGetGraphNode(child, out var node))
               return null;

          var name = GetLocalizedText(node, "schema:name");
          if (string.IsNullOrWhiteSpace(name))
               return null;

          var description = GetLocalizedTexts(node, "schema:description");
          var geo = GetGeo(child, node);
          var county = GetCountySlug(node);

          if (string.IsNullOrWhiteSpace(county) && geo.Latitude.HasValue && geo.Longitude.HasValue)
          {
               county = _countyLookupService.GetCounty(geo.Latitude.Value, geo.Longitude.Value) ?? string.Empty;
          }

          county = NormalizeCounty(county);

          return new Attraction
          {
               ExternalId = $"{contextId}:{entryId}",
               Name = name,
               DescriptionEn = description.English,
               DescriptionSv = description.Swedish,
               Website = GetIdValue(node, "schema:url"),
               County = county,
               City = GetCity(node),
               Address = BuildAddress(node),
               Category = TrimSchemaPrefix(GetString(node, "@type")),
               ImageUrl = ResolveImageUrl(child, node),
               Latitude = geo.Latitude,
               Longitude = geo.Longitude,
               LastSynced = DateTime.UtcNow
          };
     }

     private static string? NormalizeCounty(string? county)
     {
          county = county?.Trim().ToLowerInvariant();

          county = county switch
          {
               "gavleborg" => "Gävleborgs",
               "halland" => "Hallands",
               "jamtlandharjedalen" => "Jämtlands",
               "jonkoping" => "Jönköpings",
               "kronoberg" => "Kronobergs",
               "norrbotten" => "Norrbottens",
               "skane" => "Skåne",
               "stockholm" => "Stockholms",
               "sormland" => "Södermanlands",
               "varmland" => "Värmlands",
               "vasterbotten" => "Västerbottens",
               "vasternorrland" => "Västernorrlands",
               "vastmanland" => "Västmanlands",
               "vastragotaland" => "Västra Götalands",
               "orebro" => "Örebro",
               "ostergotland" => "Östergötlands",
               _ => county
          };

          return county;
     }

     private static bool TryGetGraphNode(JsonElement source, out JsonElement node)
     {
          node = default;

          JsonElement metadata;
          if (source.TryGetProperty("metadata", out metadata))
          {
               if (!metadata.TryGetProperty("@graph", out var graph) || graph.ValueKind != JsonValueKind.Array)
                    return false;

               foreach (var item in graph.EnumerateArray())
               {
                    var type = GetString(item, "@type");
                    if (type.StartsWith("schema:", StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(GetLocalizedText(item, "schema:name")))
                    {
                         node = item;
                         return true;
                    }
               }

               return false;
          }

          if (!source.TryGetProperty("@graph", out var topGraph) || topGraph.ValueKind != JsonValueKind.Array)
               return false;

          foreach (var item in topGraph.EnumerateArray())
          {
               var type = GetString(item, "@type");
               if (type.StartsWith("schema:", StringComparison.OrdinalIgnoreCase) &&
                   !string.IsNullOrWhiteSpace(GetLocalizedText(item, "schema:name")))
               {
                    node = item;
                    return true;
               }
          }

          return false;
     }

     private static string GetCountySlug(JsonElement node)
     {
          if (!node.TryGetProperty("dcterms:spatial", out var spatial) || spatial.ValueKind != JsonValueKind.Object)
               return string.Empty;

          var id = GetString(spatial, "@id");
          if (string.IsNullOrWhiteSpace(id))
               return string.Empty;

          var marker = "/region/";
          var idx = id.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
          if (idx < 0)
               return string.Empty;

          return id[(idx + marker.Length)..].Trim().ToLowerInvariant();
     }

     private static string GetCity(JsonElement node)
     {
          if (!node.TryGetProperty("schema:address", out var address))
               return string.Empty;

          if (address.ValueKind == JsonValueKind.Object)
          {
               var locality = GetLocalizedText(address, "schema:addressLocality");
               return string.IsNullOrWhiteSpace(locality) ? string.Empty : locality;
          }

          if (address.ValueKind == JsonValueKind.String)
               return address.GetString() ?? string.Empty;

          return string.Empty;
     }

     private static string BuildAddress(JsonElement node)
     {
          if (!node.TryGetProperty("schema:address", out var address))
               return string.Empty;

          if (address.ValueKind == JsonValueKind.String)
               return address.GetString() ?? string.Empty;

          if (address.ValueKind != JsonValueKind.Object)
               return string.Empty;

          var parts = new[]
          {
               GetLocalizedText(address, "schema:streetAddress"),
               GetLocalizedText(address, "schema:postalCode"),
               GetLocalizedText(address, "schema:addressLocality")
          }.Where(x => !string.IsNullOrWhiteSpace(x));

          return string.Join(", ", parts);
     }

     private static (double? Latitude, double? Longitude) GetGeo(JsonElement source, JsonElement node)
     {
          if (!node.TryGetProperty("schema:geo", out var geo) || geo.ValueKind != JsonValueKind.Object)
               return (null, null);

          var latitude = GetDouble(geo, "schema:latitude");
          var longitude = GetDouble(geo, "schema:longitude");

          if (latitude.HasValue && longitude.HasValue)
               return (latitude, longitude);

          var geoId = GetString(geo, "@id");
          if (string.IsNullOrWhiteSpace(geoId))
               return (null, null);

          if (!TryGetGraph(source, out var graph))
               return (null, null);

          foreach (var item in graph.EnumerateArray())
          {
               if (!GetString(item, "@id").Equals(geoId, StringComparison.Ordinal))
                    continue;

               latitude = GetDouble(item, "schema:latitude");
               longitude = GetDouble(item, "schema:longitude");
               return (latitude, longitude);
          }

          return (latitude, longitude);
     }

     private static bool TryGetGraph(JsonElement source, out JsonElement graph)
     {
          graph = default;

          if (source.TryGetProperty("metadata", out var metadata) &&
              metadata.TryGetProperty("@graph", out graph) &&
              graph.ValueKind == JsonValueKind.Array)
          {
               return true;
          }

          if (source.TryGetProperty("@graph", out graph) && graph.ValueKind == JsonValueKind.Array)
          {
               return true;
          }

          return false;
     }

     private static double? GetDouble(JsonElement element, string propertyName)
     {
          if (!element.TryGetProperty(propertyName, out var value))
               return null;

          if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var numberValue))
               return numberValue;

          if (value.ValueKind == JsonValueKind.String &&
              double.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var stringValue))
          {
               return stringValue;
          }

          return null;
     }

     private static string ResolveImageUrl(JsonElement source, JsonElement node)
     {
          var fromNode = GetIdValue(node, "schema:image");
          if (!string.IsNullOrWhiteSpace(fromNode))
               return fromNode;

          if (!source.TryGetProperty("metadata", out var metadata) ||
              !metadata.TryGetProperty("@graph", out var graph) ||
              graph.ValueKind != JsonValueKind.Array)
          {
               return string.Empty;
          }

          foreach (var item in graph.EnumerateArray())
          {
               if (!GetString(item, "@type").Equals("schema:ImageObject", StringComparison.OrdinalIgnoreCase))
                    continue;

               var contentUrl = GetIdValue(item, "schema:contentUrl");
               if (!string.IsNullOrWhiteSpace(contentUrl))
                    return contentUrl;
          }

          return string.Empty;
     }

     private static (string Swedish, string English) GetLocalizedTexts(JsonElement element, string propertyName)
     {
          if (!element.TryGetProperty(propertyName, out var value))
               return (string.Empty, string.Empty);

          if (value.ValueKind == JsonValueKind.String)
          {
               var single = value.GetString() ?? string.Empty;
               return (single, single);
          }

          if (value.ValueKind == JsonValueKind.Object)
          {
               var text = GetString(value, "@value");
               return (text, text);
          }

          if (value.ValueKind != JsonValueKind.Array)
               return (string.Empty, string.Empty);

          string swedish = string.Empty;
          string english = string.Empty;

          foreach (var item in value.EnumerateArray())
          {
               if (item.ValueKind == JsonValueKind.String)
               {
                    var text = item.GetString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(swedish))
                         swedish = text;
                    if (string.IsNullOrWhiteSpace(english))
                         english = text;
                    continue;
               }

               if (item.ValueKind != JsonValueKind.Object)
                    continue;

               var lang = GetString(item, "@language");
               var textValue = GetString(item, "@value");
               if (string.IsNullOrWhiteSpace(textValue))
                    continue;

               if (lang.Equals("sv", StringComparison.OrdinalIgnoreCase))
                    swedish = textValue;
               if (lang.Equals("en", StringComparison.OrdinalIgnoreCase))
                    english = textValue;
          }

          if (string.IsNullOrWhiteSpace(swedish))
               swedish = english;
          if (string.IsNullOrWhiteSpace(english))
               english = swedish;

          return (swedish, english);
     }

     private static string GetLocalizedText(JsonElement element, string propertyName)
     {
          if (!element.TryGetProperty(propertyName, out var value))
               return string.Empty;

          return GetPreferredLocalizedValue(value);
     }

     private static string GetPreferredLocalizedValue(JsonElement value)
     {
          if (value.ValueKind == JsonValueKind.String)
               return value.GetString() ?? string.Empty;

          if (value.ValueKind == JsonValueKind.Object)
               return GetString(value, "@value");

          if (value.ValueKind != JsonValueKind.Array)
               return string.Empty;

          string? english = null;
          foreach (var item in value.EnumerateArray())
          {
               if (item.ValueKind == JsonValueKind.String)
                    return item.GetString() ?? string.Empty;

               if (item.ValueKind != JsonValueKind.Object)
                    continue;

               var lang = GetString(item, "@language");
               var text = GetString(item, "@value");
               if (string.IsNullOrWhiteSpace(text))
                    continue;

               if (lang.Equals("sv", StringComparison.OrdinalIgnoreCase))
                    return text;

               if (lang.Equals("en", StringComparison.OrdinalIgnoreCase))
                    english = text;
          }

          return english ?? string.Empty;
     }

     private static string GetIdValue(JsonElement element, string propertyName)
     {
          if (!element.TryGetProperty(propertyName, out var value))
               return string.Empty;

          if (value.ValueKind == JsonValueKind.String)
               return value.GetString() ?? string.Empty;

          if (value.ValueKind == JsonValueKind.Object)
               return GetString(value, "@id");

          return string.Empty;
     }

     private static string GetString(JsonElement element, string propertyName)
     {
          if (!element.TryGetProperty(propertyName, out var value))
               return string.Empty;

          return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : string.Empty;
     }

     private static string TrimSchemaPrefix(string value)
     {
          return value.StartsWith("schema:", StringComparison.OrdinalIgnoreCase)
               ? value[7..]
               : value;
     }
}