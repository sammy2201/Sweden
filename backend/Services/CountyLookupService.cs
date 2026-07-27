using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace SwedenStart;

public class CountyLookupService : ICountyLookupService
{
     private readonly List<CountyGeometry> _counties;

     public CountyLookupService(IWebHostEnvironment environment)
     {
          var geoJsonPath = Path.Combine(environment.ContentRootPath, "Data", "geoBoundaries-SWE-ADM1.geojson");
          _counties = LoadCounties(geoJsonPath);
     }

     public string? GetCounty(double latitude, double longitude)
     {
          var point = new Point(longitude, latitude) { SRID = 4326 };

          foreach (var county in _counties)
          {
               if (county.Geometry.Contains(point))
               {
                    return county.Name;
               }
          }

          return null;
     }

     private static List<CountyGeometry> LoadCounties(string path)
     {
          if (!File.Exists(path))
          {
               throw new FileNotFoundException($"County lookup GeoJSON file not found at '{path}'.");
          }

          var geoJson = File.ReadAllText(path);
          var reader = new GeoJsonReader();
          var collection = reader.Read<FeatureCollection>(geoJson);

          var counties = new List<CountyGeometry>();

          foreach (var feature in collection)
          {
               if (feature.Geometry is not Geometry geometry)
                    continue;

               if (!feature.Attributes.Exists("shapeName"))
                    continue;

               var rawName = feature.Attributes["shapeName"]?.ToString();
               if (string.IsNullOrWhiteSpace(rawName))
                    continue;

               var normalizedName = NormalizeCountyName(rawName);
               if (string.IsNullOrWhiteSpace(normalizedName))
                    continue;

               counties.Add(new CountyGeometry(geometry, normalizedName));
          }

          return counties;
     }

     private static string NormalizeCountyName(string name)
     {
          var normalized = name.Trim();
          const string suffix = " län";

          if (normalized.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
          {
               normalized = normalized[..^suffix.Length].TrimEnd();
          }

          return normalized;
     }

     private sealed record CountyGeometry(Geometry Geometry, string Name);
}
