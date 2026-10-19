namespace SwedenStart;

public interface IVisitSwedenRepository
{
     Task<VisitSwedenUpsertResult> UpsertAttractionsRepoAsync(IEnumerable<Attraction> attractions);
}
