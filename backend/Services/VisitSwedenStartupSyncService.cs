using Microsoft.EntityFrameworkCore;

namespace SwedenStart;

public sealed class VisitSwedenStartupSyncService : BackgroundService
{
     private readonly IServiceScopeFactory _scopeFactory;
     private readonly ILogger<VisitSwedenStartupSyncService> _logger;

     public VisitSwedenStartupSyncService(
          IServiceScopeFactory scopeFactory,
          ILogger<VisitSwedenStartupSyncService> logger)
     {
          _scopeFactory = scopeFactory;
          _logger = logger;
     }

     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
     {
          await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

          const int maxAttempts = 3;
          for (var attempt = 1; attempt <= maxAttempts; attempt++)
          {
               try
               {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    if (await dbContext.Attractions.AnyAsync(stoppingToken))
                    {
                         _logger.LogInformation("Visit Sweden startup sync skipped because attraction data already exists.");
                         return;
                    }

                    _logger.LogInformation(
                         "Visit Sweden startup sync started because the local attraction store is empty (attempt {Attempt}/{MaxAttempts}).",
                         attempt,
                         maxAttempts);
                    var visitSwedenService = scope.ServiceProvider.GetRequiredService<IVisitSwedenService>();
                    var result = await visitSwedenService.SyncAttractionsAsync(stoppingToken);
                    _logger.LogInformation(
                         "Visit Sweden startup sync finished successfully. Attempts: {Attempt}/{MaxAttempts}; fetched: {Fetched}; mapped: {Mapped}; inserted: {Inserted}; updated: {Updated}; skipped: {Skipped}.",
                         attempt,
                         maxAttempts,
                         result.RecordsFetched,
                         result.AttractionsMapped,
                         result.AttractionsInserted,
                         result.AttractionsUpdated,
                         result.SkippedMissingImage + result.SkippedMissingCoordinates + result.SkippedUnmappable);
                    return;
               }
               catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
               {
                    return;
               }
               catch (Exception exception) when (attempt < maxAttempts)
               {
                    _logger.LogWarning(
                         exception,
                         "Visit Sweden startup sync attempt {Attempt}/{MaxAttempts} failed. Retrying in 30 seconds.",
                         attempt,
                         maxAttempts);
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
               }
               catch (Exception exception)
               {
                    _logger.LogError(
                         exception,
                         "Visit Sweden startup sync failed after {MaxAttempts} attempts. The API remains available; an administrator can retry with POST /api/visitsweden/sync.",
                         maxAttempts);
               }
          }
     }
}
