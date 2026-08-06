namespace SwedenStart;

public interface IVisitSwedenRepository
{
     Task UpsertAttractionsAsync(IEnumerable<Attraction> attractions);
}
