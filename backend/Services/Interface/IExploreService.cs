namespace SwedenStart;

public interface IExploreService
{
     IEnumerable<CountyDto> GetCounties();

     Task<bool> HasAnyAttractionsAsync(CancellationToken cancellationToken = default);

     Task<PagedResponse<AttractionDto>> GetAttractionsAsync(string county, int page, int pageSize, string? category);

     Task<AttractionDetailDto?> GetAttractionAsync(string id);
}
