using Npgsql;
namespace SwedenStart.Health;

public class HealthService : IHealthService
{
   private readonly NpgsqlDataSource _dataSource;
   public HealthService(NpgsqlDataSource dataSource)
{
    _dataSource = dataSource;
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
        catch
        {
            return new HealthResponseDto
            {
                Status = "Unhealthy",
                Database = "Unhealthy"
            };
        }
    }
}