namespace SwedenStart;

public interface ICountyLookupService
{
     string? GetCounty(double latitude, double longitude);
}
