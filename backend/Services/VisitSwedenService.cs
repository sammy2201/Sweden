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
     private readonly VisitSwedenSyncGate _syncGate;

     public VisitSwedenService(
         HttpClient httpClient,
         ILogger<VisitSwedenService> logger,
         IVisitSwedenRepository visitSwedenRepository,
         VisitSwedenSyncGate syncGate)
     {
          _httpClient = httpClient;
          _logger = logger;
          _visitSwedenRepository = visitSwedenRepository;
          _syncGate = syncGate;
     }

     public async Task<VisitSwedenSyncResult> SyncAttractionsAsync(CancellationToken cancellationToken = default)
     {
          await _syncGate.WaitAsync(cancellationToken);
          try
          {
               return await SyncAttractionsCoreAsync(cancellationToken);
          }
          finally
          {
               _syncGate.Release();
          }
     }

     private async Task<VisitSwedenSyncResult> SyncAttractionsCoreAsync(CancellationToken cancellationToken)
     {
          var mapped = new List<Attraction>();
          var skippedMissingImage = 0;
          var skippedMissingCoordinates = 0;
          var skippedTotal = 0;
          var skippedUnmappable = 0;
          var pagesFetched = 0;
          var recordsFetched = 0;
          var currentOffset = 0;
          int? sourceTotal = null;

          try
          {
               for (var offset = 0; ; offset += SourcePageSize)
               {
                    currentOffset = offset;
                    var query = "public:true AND rdfType:http\\://schema.org/*";
                    var endpoint =
                         $"search?type=solr&query={Uri.EscapeDataString(query)}&limit={SourcePageSize}&offset={offset}&rdfFormat=application/ld+json";

                    HttpResponseMessage response;
                    try
                    {
                         response = await _httpClient.GetAsync(endpoint, cancellationToken);
                    }
                    catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
                    {
                         _logger.LogWarning("Visit Sweden sync request timed out at offset {Offset}.", offset);
                         throw new HttpRequestException("Visit Sweden API request timed out.", exception);
                    }

                    using var responseScope = response;
                    if (!response.IsSuccessStatusCode)
                    {
                         _logger.LogWarning(
                              "Visit Sweden sync request failed at offset {Offset} with HTTP {StatusCode}.",
                              offset,
                              (int)response.StatusCode);
                         throw new HttpRequestException(
                              $"Visit Sweden API request failed with HTTP {(int)response.StatusCode}.",
                              null,
                              response.StatusCode);
                    }

                    JsonDocument document;
                    try
                    {
                         await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                         document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                    }
                    catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
                    {
                         _logger.LogWarning("Visit Sweden response timed out while reading offset {Offset}.", offset);
                         throw new HttpRequestException("Visit Sweden API response timed out.", exception);
                    }
                    catch (JsonException exception)
                    {
                         _logger.LogWarning(exception, "Visit Sweden returned malformed JSON at offset {Offset}.", offset);
                         throw new HttpRequestException("Visit Sweden API returned an invalid response.", exception);
                    }

                    using (document)
                    {
                         var children = GetChildren(document.RootElement);
                         if (!document.RootElement.TryGetProperty("results", out var resultsElement) ||
                             !resultsElement.TryGetInt32(out var pageTotal))
                         {
                              throw new HttpRequestException("Visit Sweden response did not include the expected result count.");
                         }

                         sourceTotal ??= pageTotal;
                         if (children.Count == 0)
                         {
                              if (recordsFetched < sourceTotal)
                              {
                                   throw new HttpRequestException(
                                        $"Visit Sweden returned no records at offset {offset} before the reported result count was reached.");
                              }

                              break;
                         }

                         pagesFetched++;
                         recordsFetched += children.Count;

                         foreach (var child in children)
                         {
                              var attraction = MapAttraction(child);
                              if (attraction is null)
                              {
                                   skippedUnmappable++;
                                   continue;
                              }

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

                              if (string.IsNullOrWhiteSpace(attraction.County))
                              {
                                   skippedUnmappable++;
                                   continue;
                              }

                              mapped.Add(attraction);
                         }

                         if (recordsFetched >= sourceTotal)
                         {
                              break;
                         }

                         if (children.Count < SourcePageSize)
                         {
                              throw new HttpRequestException(
                                   $"Visit Sweden returned a short page at offset {offset} before the reported result count was reached.");
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

               if (recordsFetched > 0 && mapped.Count == 0)
               {
                    _logger.LogWarning(
                         "Visit Sweden returned {RecordsFetched} source records, but none could be mapped to a county attraction.",
                         recordsFetched);
                    throw new HttpRequestException("Visit Sweden returned records, but none could be mapped to usable county attractions.");
               }

               var upsertResult = await _visitSwedenRepository.UpsertAttractionsAsync(mapped, cancellationToken);
               var result = new VisitSwedenSyncResult(
                    pagesFetched,
                    recordsFetched,
                    mapped.Count,
                    upsertResult.Inserted,
                    upsertResult.Updated,
                    skippedMissingImage,
                    skippedMissingCoordinates,
                    skippedUnmappable);

               _logger.LogInformation(
                    "Visit Sweden sync completed. Pages: {PagesFetched}, source records: {RecordsFetched}/{SourceTotal}, mapped: {MappedCount}, inserted: {InsertedCount}, updated: {UpdatedCount}, missing image: {MissingImageCount}, missing coordinates: {MissingCoordinatesCount}, unmappable: {UnmappableCount}.",
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
          catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
          {
               _logger.LogError(
                    exception,
                    "Visit Sweden sync failed at offset {Offset}. Pages fetched: {PagesFetched}; source records fetched: {RecordsFetched}/{SourceTotal}; mapped: {MappedCount}; skipped missing image: {MissingImageCount}; skipped missing coordinates: {MissingCoordinatesCount}; skipped unmappable: {UnmappableCount}.",
                    currentOffset,
                    pagesFetched,
                    recordsFetched,
                    sourceTotal,
                    mapped.Count,
                    skippedMissingImage,
                    skippedMissingCoordinates,
                    skippedUnmappable);
               throw;
          }
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
              resource.ValueKind != JsonValueKind.Object ||
              !resource.TryGetProperty("children", out var children) ||
              children.ValueKind != JsonValueKind.Array)
          {
               throw new HttpRequestException("Visit Sweden response did not include the expected resource children array.");
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
          var county = CountyData.ResolveCountyId(GetCountySlug(node));

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
