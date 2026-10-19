
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SwedenStart;

public class VisitSwedenService : IVisitSwedenService
{
     private const int SourcePageSize = 100;

     private readonly HttpClient _httpClient;
     private readonly ILogger<VisitSwedenService> _logger;
     private readonly IVisitSwedenRepository _repository;


     public VisitSwedenService(
         HttpClient httpClient,
         ILogger<VisitSwedenService> logger,
         IVisitSwedenRepository repository
     )
     {
          _httpClient = httpClient;
          _logger = logger;
          _repository = repository;

     }
     public Task<VisitSwedenSyncResult> SyncAttractionsServiceAsync()
     {
          return SyncAttractionsCoreAsync();
     }

     private async Task<VisitSwedenSyncResult> SyncAttractionsCoreAsync()
     {
          var attractions = new List<Attraction>();
          var skippedMissingImage = 0;
          var skippedMissingCoordinates = 0;
          var skippedUnmappable = 0;
          var pagesFetched = 0;
          var recordsFetched = 0;
          int? sourceTotal = null;

          for (var offset = 0; ; offset += SourcePageSize)
          {
               var query = "public:true AND rdfType:http\\://schema.org/*";
               var endpoint =
                   $"search?type=solr&query={Uri.EscapeDataString(query)}" +
                   $"&limit={SourcePageSize}&offset={offset}" +
                   "&rdfFormat=application/ld+json";

               using var response = await _httpClient.GetAsync(endpoint);

               if (!response.IsSuccessStatusCode)
               {
                    throw new HttpRequestException(
                        $"Visit Sweden API request failed with HTTP {(int)response.StatusCode}.",
                        null,
                        response.StatusCode);
               }

               using var document = await ReadResponseAsync(response, offset);
               var root = document.RootElement;

               if (!root.TryGetProperty("results", out var results) ||
                   !results.TryGetInt32(out var pageTotal))
               {
                    throw new HttpRequestException(
                        "Visit Sweden response did not include the expected result count.");
               }

               sourceTotal ??= pageTotal;

               var children = GetChildren(root);

               if (children.Count == 0)
               {
                    if (recordsFetched < sourceTotal.Value)
                    {
                         throw new HttpRequestException(
                             $"Visit Sweden returned no records at offset {offset} " +
                             "before the reported result count was reached.");
                    }

                    break;
               }

               pagesFetched++;
               recordsFetched += children.Count;

               foreach (var child in children)
               {
                    var attraction = MapAttraction(child);

                    if (attraction is null ||
                        string.IsNullOrWhiteSpace(attraction.County))
                    {
                         skippedUnmappable++;
                         continue;
                    }

                    var missingImage = string.IsNullOrWhiteSpace(attraction.ImageUrl);
                    var missingCoordinates = !HasValidCoordinates(
                        attraction.Latitude,
                        attraction.Longitude);

                    if (missingImage || missingCoordinates)
                    {
                         if (missingImage)
                              skippedMissingImage++;

                         if (missingCoordinates)
                              skippedMissingCoordinates++;

                         continue;
                    }

                    attractions.Add(attraction);
               }

               if (recordsFetched >= sourceTotal.Value)
                    break;

               if (children.Count < SourcePageSize)
               {
                    throw new HttpRequestException(
                        $"Visit Sweden returned a short page at offset {offset} " +
                        "before the reported result count was reached.");
               }
          }

          if (recordsFetched > 0 && attractions.Count == 0)
          {
               throw new HttpRequestException(
                   "Visit Sweden returned records, but none could be mapped " +
                   "to usable county attractions.");
          }

          var upsert = await _repository.UpsertAttractionsRepoAsync(attractions);

          var result = new VisitSwedenSyncResult(
              pagesFetched,
              recordsFetched,
              attractions.Count,
              upsert.Inserted,
              upsert.Updated,
              skippedMissingImage,
              skippedMissingCoordinates,
              skippedUnmappable);

          _logger.LogInformation(
              "Visit Sweden sync completed. Pages: {Pages}, records: {Records}/{Total}, " +
              "mapped: {Mapped}, inserted: {Inserted}, updated: {Updated}, " +
              "missing image: {MissingImage}, missing coordinates: {MissingCoordinates}, " +
              "unmappable: {Unmappable}.",
              result.PagesFetched,
              result.RecordsFetched,
              sourceTotal,
              result.AttractionsMapped,
              result.AttractionsInserted,
              result.AttractionsUpdated,
              result.SkippedMissingImage,
              result.SkippedMissingCoordinates,
              result.SkippedUnmappable);

          return result;
     }

     private async Task<JsonDocument> ReadResponseAsync(
         HttpResponseMessage response,
         int offset)
     {
          try
          {
               await using var stream = await response.Content.ReadAsStreamAsync();
               return await JsonDocument.ParseAsync(stream);
          }
          catch (JsonException exception)
          {
               _logger.LogWarning(
                   exception,
                   "Visit Sweden returned malformed JSON at offset {Offset}.",
                   offset);

               throw new HttpRequestException(
                   "Visit Sweden API returned an invalid response.",
                   exception);
          }
     }

     private static List<JsonElement> GetChildren(JsonElement root)
     {
          if (!root.TryGetProperty("resource", out var resource) ||
              resource.ValueKind != JsonValueKind.Object ||
              !resource.TryGetProperty("children", out var children) ||
              children.ValueKind != JsonValueKind.Array)
          {
               throw new HttpRequestException(
                   "Visit Sweden response did not include the expected resource children array.");
          }

          return children.EnumerateArray().ToList();
     }

     private static bool HasValidCoordinates(double? latitude, double? longitude)
     {
          return latitude.HasValue &&
                 longitude.HasValue &&
                 double.IsFinite(latitude.Value) &&
                 double.IsFinite(longitude.Value) &&
                 latitude.Value is >= -90 and <= 90 &&
                 longitude.Value is >= -180 and <= 180;
     }

     private static Attraction? MapAttraction(JsonElement child)
     {
          var contextId = GetString(child, "contextId");
          var entryId = GetString(child, "entryId");

          if (string.IsNullOrWhiteSpace(contextId) ||
              string.IsNullOrWhiteSpace(entryId) ||
              !TryGetGraphNode(child, out var node))
          {
               return null;
          }

          var name = GetLocalizedText(node, "schema:name");

          if (string.IsNullOrWhiteSpace(name))
               return null;

          var description = GetLocalizedTexts(node, "schema:description");
          var coordinates = GetGeo(child, node);

          return new Attraction
          {
               ExternalId = $"{contextId}:{entryId}",
               Name = name,
               DescriptionEn = description.English,
               DescriptionSv = description.Swedish,
               Website = GetIdValue(node, "schema:url"),
               County = CountyData.ResolveCountyId(GetCountySlug(node)),
               City = GetCity(node),
               Address = BuildAddress(node),
               Category = TrimSchemaPrefix(GetString(node, "@type")),
               ImageUrl = ResolveImageUrl(child, node),
               Latitude = coordinates.Latitude,
               Longitude = coordinates.Longitude,
               LastSynced = DateTime.UtcNow
          };
     }

     private static bool TryGetGraphNode(
         JsonElement source,
         out JsonElement node)
     {
          node = default;

          if (!TryGetGraph(source, out var graph))
               return false;

          foreach (var item in graph.EnumerateArray())
          {
               if (GetString(item, "@type")
                       .StartsWith("schema:", StringComparison.OrdinalIgnoreCase) &&
                   !string.IsNullOrWhiteSpace(GetLocalizedText(item, "schema:name")))
               {
                    node = item;
                    return true;
               }
          }

          return false;
     }

     private static bool TryGetGraph(
         JsonElement source,
         out JsonElement graph)
     {
          graph = default;

          if (source.TryGetProperty("metadata", out var metadata) &&
              metadata.ValueKind == JsonValueKind.Object &&
              metadata.TryGetProperty("@graph", out graph) &&
              graph.ValueKind == JsonValueKind.Array)
          {
               return true;
          }

          if (source.TryGetProperty("@graph", out graph) &&
              graph.ValueKind == JsonValueKind.Array)
          {
               return true;
          }

          return false;
     }

     private static string GetCountySlug(JsonElement node)
     {
          if (!node.TryGetProperty("dcterms:spatial", out var spatial) ||
              spatial.ValueKind != JsonValueKind.Object)
          {
               return string.Empty;
          }

          var identifier = GetString(spatial, "@id");
          const string marker = "/region/";

          var position = identifier.IndexOf(
              marker,
              StringComparison.OrdinalIgnoreCase);

          return position < 0
              ? string.Empty
              : identifier[(position + marker.Length)..].Trim().ToLowerInvariant();
     }

     private static string GetCity(JsonElement node)
     {
          if (!node.TryGetProperty("schema:address", out var address))
               return string.Empty;

          if (address.ValueKind == JsonValueKind.Object)
               return GetLocalizedText(address, "schema:addressLocality");

          return address.ValueKind == JsonValueKind.String
              ? address.GetString() ?? string.Empty
              : string.Empty;
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
        };

          return string.Join(
              ", ",
              parts.Where(part => !string.IsNullOrWhiteSpace(part)));
     }

     private static (double? Latitude, double? Longitude) GetGeo(
         JsonElement source,
         JsonElement node)
     {
          if (!node.TryGetProperty("schema:geo", out var geo) ||
              geo.ValueKind != JsonValueKind.Object)
          {
               return (null, null);
          }

          var latitude = GetDouble(geo, "schema:latitude");
          var longitude = GetDouble(geo, "schema:longitude");

          if (latitude.HasValue && longitude.HasValue)
               return (latitude, longitude);

          var identifier = GetString(geo, "@id");

          if (string.IsNullOrWhiteSpace(identifier) ||
              !TryGetGraph(source, out var graph))
          {
               return (latitude, longitude);
          }

          foreach (var item in graph.EnumerateArray())
          {
               if (GetString(item, "@id") != identifier)
                    continue;

               return (
                   GetDouble(item, "schema:latitude"),
                   GetDouble(item, "schema:longitude"));
          }

          return (latitude, longitude);
     }

     private static double? GetDouble(
         JsonElement element,
         string propertyName)
     {
          if (!element.TryGetProperty(propertyName, out var value))
               return null;

          if (value.ValueKind == JsonValueKind.Number &&
              value.TryGetDouble(out var number))
          {
               return number;
          }

          if (value.ValueKind == JsonValueKind.String &&
              double.TryParse(
                  value.GetString(),
                  NumberStyles.Any,
                  CultureInfo.InvariantCulture,
                  out number))
          {
               return number;
          }

          return null;
     }

     private static string ResolveImageUrl(
         JsonElement source,
         JsonElement node)
     {
          var imageUrl = GetIdValue(node, "schema:image");

          if (!string.IsNullOrWhiteSpace(imageUrl))
               return imageUrl;

          if (!TryGetGraph(source, out var graph))
               return string.Empty;

          foreach (var item in graph.EnumerateArray())
          {
               if (!GetString(item, "@type")
                       .Equals("schema:ImageObject", StringComparison.OrdinalIgnoreCase))
               {
                    continue;
               }

               imageUrl = GetIdValue(item, "schema:contentUrl");

               if (!string.IsNullOrWhiteSpace(imageUrl))
                    return imageUrl;
          }

          return string.Empty;
     }

     private static (string Swedish, string English) GetLocalizedTexts(
         JsonElement element,
         string propertyName)
     {
          if (!element.TryGetProperty(propertyName, out var value))
               return (string.Empty, string.Empty);

          if (value.ValueKind == JsonValueKind.String ||
              value.ValueKind == JsonValueKind.Object)
          {
               var text = value.ValueKind == JsonValueKind.String
                   ? value.GetString() ?? string.Empty
                   : GetString(value, "@value");

               return (text, text);
          }

          if (value.ValueKind != JsonValueKind.Array)
               return (string.Empty, string.Empty);

          var swedish = string.Empty;
          var english = string.Empty;

          foreach (var item in value.EnumerateArray())
          {
               if (item.ValueKind == JsonValueKind.String)
               {
                    var text = item.GetString() ?? string.Empty;
                    swedish = string.IsNullOrWhiteSpace(swedish) ? text : swedish;
                    english = string.IsNullOrWhiteSpace(english) ? text : english;
                    continue;
               }

               if (item.ValueKind != JsonValueKind.Object)
                    continue;

               var language = GetString(item, "@language");
               var textValue = GetString(item, "@value");

               if (string.IsNullOrWhiteSpace(textValue))
                    continue;

               if (language.Equals("sv", StringComparison.OrdinalIgnoreCase))
                    swedish = textValue;
               else if (language.Equals("en", StringComparison.OrdinalIgnoreCase))
                    english = textValue;
          }

          return (
              string.IsNullOrWhiteSpace(swedish) ? english : swedish,
              string.IsNullOrWhiteSpace(english) ? swedish : english);
     }

     private static string GetLocalizedText(
         JsonElement element,
         string propertyName)
     {
          if (!element.TryGetProperty(propertyName, out var value))
               return string.Empty;

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

               var language = GetString(item, "@language");
               var text = GetString(item, "@value");

               if (string.IsNullOrWhiteSpace(text))
                    continue;

               if (language.Equals("sv", StringComparison.OrdinalIgnoreCase))
                    return text;

               if (language.Equals("en", StringComparison.OrdinalIgnoreCase))
                    english = text;
          }

          return english ?? string.Empty;
     }

     private static string GetIdValue(
         JsonElement element,
         string propertyName)
     {
          if (!element.TryGetProperty(propertyName, out var value))
               return string.Empty;

          if (value.ValueKind == JsonValueKind.String)
               return value.GetString() ?? string.Empty;

          return value.ValueKind == JsonValueKind.Object
              ? GetString(value, "@id")
              : string.Empty;
     }

     private static string GetString(
         JsonElement element,
         string propertyName)
     {
          if (!element.TryGetProperty(propertyName, out var value))
               return string.Empty;

          return value.ValueKind == JsonValueKind.String
              ? value.GetString() ?? string.Empty
              : string.Empty;
     }

     private static string TrimSchemaPrefix(string value)
     {
          return value.StartsWith("schema:", StringComparison.OrdinalIgnoreCase)
              ? value[7..]
              : value;
     }
}
