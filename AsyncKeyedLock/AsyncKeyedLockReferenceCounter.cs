using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class AsyncKeyedLockReferenceCounter<TKey>
    {
        private readonly TKey _key;
        public TKey Key => _key;

        public int ReferenceCount { get; set; }

        private readonly SemaphoreSlim _semaphoreSlim;
        public SemaphoreSlim SemaphoreSlim => _semaphoreSlim;

        public AsyncKeyedLockReleaser<TKey> Releaser;

        public AsyncKeyedLockReferenceCounter(TKey key, SemaphoreSlim semaphoreSlim, AsyncKeyedLockerDictionary<TKey> dictionary)
        {
            _key = key;
            _semaphoreSlim = semaphoreSlim;
            Releaser = new AsyncKeyedLockReleaser<TKey>(dictionary, this);
        }
    }
}
