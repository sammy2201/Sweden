using Microsoft.EntityFrameworkCore;

namespace SwedenStart;

public class VisitSwedenRepository : IVisitSwedenRepository
{
     private readonly AppDbContext _dbContext;

     public VisitSwedenRepository(AppDbContext dbContext)
     {
          _dbContext = dbContext;
     }

     public async Task<VisitSwedenUpsertResult> UpsertAttractionsRepoAsync(IEnumerable<Attraction> attractions)
     {
          var incoming = attractions
               .Where(attraction => !string.IsNullOrWhiteSpace(attraction.ExternalId))
               .GroupBy(attraction => attraction.ExternalId)
               .Select(duplicateGroup => duplicateGroup.First())
               .ToList();

          if (incoming.Count == 0)
               return new VisitSwedenUpsertResult(0, 0);

          var externalIds = incoming.Select(attraction => attraction.ExternalId).ToList();

          var existingByExternalId = await _dbContext.Attractions
               .Where(attraction => externalIds.Contains(attraction.ExternalId))
               .ToDictionaryAsync(attraction => attraction.ExternalId);

          var inserted = 0;
          var updated = 0;
          foreach (var item in incoming)
          {
               if (existingByExternalId.TryGetValue(item.ExternalId, out var existing))
               {
                    updated++;
                    existing.Name = item.Name;
                    existing.DescriptionEn = item.DescriptionEn;
                    existing.DescriptionSv = item.DescriptionSv;
                    existing.Website = item.Website;
                    existing.County = item.County;
                    existing.City = item.City;
                    existing.Address = item.Address;
                    existing.Category = item.Category;
                    existing.ImageUrl = item.ImageUrl;
                    existing.Latitude = item.Latitude;
                    existing.Longitude = item.Longitude;
                    existing.LastSynced = DateTime.UtcNow;
               }
               else
               {
                    item.LastSynced = DateTime.UtcNow;
                    _dbContext.Attractions.Add(item);
                    inserted++;
               }
          }

          await _dbContext.SaveChangesAsync();
          return new VisitSwedenUpsertResult(inserted, updated);
     }
}
