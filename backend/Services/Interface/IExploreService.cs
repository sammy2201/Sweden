namespace SwedenStart;

public interface IExploreService
{
     IEnumerable<CountyDto> GetCounties();

     Task<bool> HasAnyAttractionsAsync();

     Task<PagedResponse<AttractionDto>> GetAttractionsAsync(string county, int page, int pageSize, string? category);

     Task<AttractionDetailDto?> GetAttractionAsync(string id);
}
