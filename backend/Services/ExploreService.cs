namespace SwedenStart;

public class ExploreService : IExploreService
{
     private readonly IAttractionRepository _attractionRepository;

     public ExploreService(IAttractionRepository attractionRepository)
     {
          _attractionRepository = attractionRepository;
     }

     public async Task<IEnumerable<CountyDto>> GetCountiesAsync()
     {
          return await CountyData.GetCountiesAsync();
     }

     public async Task<PagedResponse<AttractionDto>> GetAttractionsAsync(
         string county,
         int page,
         int pageSize,
           string? category)
     {
          var attractions = await _attractionRepository.GetAttractionsAsync(
              county,
              page,
              pageSize,
                 category ?? string.Empty);

          return new PagedResponse<AttractionDto>
          {
               Items = attractions.Items.Select(a => new AttractionDto
               {
                    Id = a.ExternalId,
                    Name = a.Name,
                    City = a.City ?? string.Empty,
                    Category = a.Category ?? string.Empty,
                    ImageUrl = a.ImageUrl ?? string.Empty
               }).ToList(),
               Page = attractions.Page,
               PageSize = attractions.PageSize,
               TotalCount = attractions.TotalCount
          };
     }

     public async Task<AttractionDetailDto?> GetAttractionAsync(string id)
     {
          var attraction = await _attractionRepository.GetAttractionByIdAsync(id);

          if (attraction is null)
               return null;

          return new AttractionDetailDto
          {
               Id = attraction.ExternalId,
               Name = attraction.Name,
               Description = attraction.DescriptionEn ?? string.Empty,
               Address = attraction.Address ?? string.Empty,
               Website = attraction.Website ?? string.Empty,
               ImageUrl = attraction.ImageUrl ?? string.Empty,
               Category = attraction.Category ?? string.Empty
          };
     }
}