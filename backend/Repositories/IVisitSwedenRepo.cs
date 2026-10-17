namespace SwedenStart;

public interface IVisitSwedenRepository
{
     Task<VisitSwedenUpsertResult> UpsertAttractionsAsync(IEnumerable<Attraction> attractions, CancellationToken cancellationToken = default);
}
