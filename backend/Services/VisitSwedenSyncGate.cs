namespace SwedenStart;

public sealed class VisitSwedenSyncGate
{
     private readonly SemaphoreSlim _semaphore = new(1, 1);

     public Task WaitAsync(CancellationToken cancellationToken) => _semaphore.WaitAsync(cancellationToken);

     public void Release() => _semaphore.Release();
}
