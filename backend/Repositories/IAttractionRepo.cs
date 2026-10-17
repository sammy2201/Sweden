namespace SwedenStart;

public interface IAttractionRepository
{
     Task<bool> HasAnyAttractionsAsync(CancellationToken cancellationToken = default);

     Task<PagedResponse<Attraction>> GetAttractionsAsync(
         string? county,
         int page,
         int pageSize,
         string? category);

     Task<Attraction?> GetAttractionByIdAsync(string externalId);
}
