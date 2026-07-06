namespace SwedenStart.Health;

public interface IHealthService
{
    Task<HealthResponseDto> GetHealthAsync();
}