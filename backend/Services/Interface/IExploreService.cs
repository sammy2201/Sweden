namespace SwedenStart;

public interface IExploreService
{
     Task<IEnumerable<CountyDto>> GetCountiesAsync();

     Task<PagedResponse<AttractionDto>> GetAttractionsAsync(string county, int page, int pageSize, string? category);

     Task<AttractionDetailDto?> GetAttractionAsync(string id);
}