namespace SwedenStart;

public class VisitSwedenSyncGate
{
     private readonly SemaphoreSlim _semaphore = new(1, 1);

     public Task WaitAsync() => _semaphore.WaitAsync();

     public void Release() => _semaphore.Release();
}
