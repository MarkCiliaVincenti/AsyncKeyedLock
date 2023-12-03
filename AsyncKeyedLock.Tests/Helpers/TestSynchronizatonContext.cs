
namespace AsyncKeyedLock.Tests.Helpers
{
    public class TestSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state)
        {
            d(state);
        }
    }
}