namespace SwedenStart;

public interface IExploreService
{


     Task<PagedResponse<AttractionDto>> GetAttractionsServiceAsync(string county, int page, int pageSize, string? category);

     Task<AttractionDetailDto?> GetAttractionServiceAsync(string id);
}
