using Microsoft.EntityFrameworkCore;

namespace SwedenStart;

public class AttractionRepository : IAttractionRepository
{
     private readonly AppDbContext _dbContext;

     public AttractionRepository(AppDbContext dbContext)
     {
          _dbContext = dbContext;
     }

     public async Task<PagedResponse<Attraction>> GetAttractionsAsync(
         string? county,
         int page,
         int pageSize,
         string? category)
     {
          var query = _dbContext.Attractions.AsQueryable();

          if (!string.IsNullOrWhiteSpace(county))
          {
               query = query.Where(a => a.County == county);
          }


          if (!string.IsNullOrWhiteSpace(category))
          {
               query = query.Where(a => a.Category == category);
          }
          var totalCount = await query.CountAsync();

          var items = await query
              .OrderBy(a => a.Name)
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
              .FirstOrDefaultAsync(a => a.ExternalId == externalId);
     }
}