using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class ReferenceCounter<TKey>
    {
        private readonly TKey _key;
        public TKey Key => _key;

        public int ReferenceCount { get; set; }

        private readonly SemaphoreSlim _semaphoreSlim;
        public SemaphoreSlim SemaphoreSlim => _semaphoreSlim;

        public ReferenceCounter(TKey key, SemaphoreSlim semaphoreSlim)
        {
            _key = key;
            _semaphoreSlim = semaphoreSlim;
        }
    }
}
