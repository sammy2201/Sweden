using Microsoft.EntityFrameworkCore;

namespace SwedenStart;

public class AttractionRepository : IAttractionRepository
{
     private readonly AppDbContext _dbContext;

     public AttractionRepository(AppDbContext dbContext)
     {
          _dbContext = dbContext;
     }

     public Task<bool> HasAnyAttractionsAsync(CancellationToken cancellationToken = default)
          => _dbContext.Attractions.AnyAsync(cancellationToken);

     public async Task<PagedResponse<Attraction>> GetAttractionsAsync(
         string? county,
         int page,
         int pageSize,
         string? category)
     {
          var query = _dbContext.Attractions.AsQueryable();

          if (!string.IsNullOrWhiteSpace(county))
          {
               var countyId = CountyData.ResolveCountyId(county);
               var countyValues = CountyData.GetCountyFilterValues(county);
               query = string.IsNullOrWhiteSpace(countyId)
                    ? query.Where(attraction => countyValues.Contains(attraction.County ?? string.Empty))
                    : query.Where(attraction => attraction.County == countyId || countyValues.Contains(attraction.County ?? string.Empty));
          }


          if (!string.IsNullOrWhiteSpace(category))
          {
               query = query.Where(attraction => attraction.Category == category);
          }
          var totalCount = await query.CountAsync();

          var items = await query
              .OrderBy(attraction => attraction.Name)
              .Skip((page - 1) * pageSize)
              .Take(pageSize)
              .ToListAsync();

          return new PagedResponse<Attraction>
          {
               Items = items,
               Page = page,
               PageSize = pageSize,
               TotalCount = totalCount
          };
     }

     public async Task<Attraction?> GetAttractionByIdAsync(string externalId)
     {
          return await _dbContext.Attractions
              .FirstOrDefaultAsync(attraction => attraction.ExternalId == externalId);
     }
}
