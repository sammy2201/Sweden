namespace SwedenStart;

public interface IHealthService
{
    Task<HealthResponseDto> GetHealthAsync();
}