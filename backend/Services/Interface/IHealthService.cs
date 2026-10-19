namespace SwedenStart;

public interface IHealthService
{
    Task<HealthResponseDto> GetHealthServiceAsync();
}