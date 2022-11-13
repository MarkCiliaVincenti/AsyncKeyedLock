using System;
using System.Threading;

namespace AsyncKeyedLock
{
    internal class AsyncKeyedLockReleaser<TKey> : IAsyncKeyedLockReleaser<TKey>
    {
        private readonly TKey _key;
        public TKey Key => _key;

        private int _referenceCount = 1;

        public int ReferenceCount
        {
            get => _referenceCount;
            set => _referenceCount = value;
        }

        private readonly SemaphoreSlim _semaphoreSlim;
        public SemaphoreSlim SemaphoreSlim => _semaphoreSlim;

        private readonly AsyncKeyedLockerDictionary<TKey> _dictionary;

        public AsyncKeyedLockReleaser(TKey key, SemaphoreSlim semaphoreSlim, AsyncKeyedLockerDictionary<TKey> dictionary)
        {
            _key = key;
            _semaphoreSlim = semaphoreSlim;
            _dictionary = dictionary;
        }

        public bool TryIncrement()
        {
            if (Monitor.TryEnter(this))
            {
                _referenceCount++;
                Monitor.Exit(this);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            _dictionary.Release(this);
        }
    }
}