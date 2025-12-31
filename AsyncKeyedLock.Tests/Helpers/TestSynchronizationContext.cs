namespace AsyncKeyedLock.Tests.Helpers;

internal class TestSynchronizationContext : SynchronizationContext
{
    public int LastPostThreadId { get; private set; }

    public override void Post(SendOrPostCallback d, object? state)
    {
        LastPostThreadId = Environment.CurrentManagedThreadId;
        d(state);
    }
}
