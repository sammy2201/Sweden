namespace SwedenStart;

public interface IAttractionRepository
{
    Task<bool> HasAnyAttractionsRepoAsync();

    Task<PagedResponse<Attraction>> GetAttractionsRepoAsync(
        string? county,
        int page,
        int pageSize,
        string? category);

    Task<Attraction?> GetAttractionByIdRepoAsync(string externalId);
}
