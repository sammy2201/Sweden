using Microsoft.Extensions.Logging;
using Npgsql;
namespace SwedenStart;

public class HealthService : IHealthService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<HealthService> _logger;

    public HealthService(NpgsqlDataSource dataSource, ILogger<HealthService> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task<HealthResponseDto> GetHealthAsync()
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            return new HealthResponseDto
            {
                Status = "Healthy",
                Database = "Healthy"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed.");
            return new HealthResponseDto
            {
                Status = "Unhealthy",
                Database = "Unhealthy"
            };
        }
    }
}