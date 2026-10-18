namespace SwedenStart;

public interface IAttractionRepository
{
    Task<bool> HasAnyAttractionsAsync();

    Task<PagedResponse<Attraction>> GetAttractionsAsync(
        string? county,
        int page,
        int pageSize,
        string? category);

    Task<Attraction?> GetAttractionByIdAsync(string externalId);
}
