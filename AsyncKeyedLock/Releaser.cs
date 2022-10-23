using System;
using System.Threading;

namespace AsyncKeyedLock
{
    internal struct Releaser<TKey> : IDisposable
    {
        private readonly AsyncKeyedLocker<TKey> _asyncKeyedLocker;
        private readonly ReferenceCounter<TKey> _referenceCounter;

        public Releaser(AsyncKeyedLocker<TKey> asyncKeyedLocker, ReferenceCounter<TKey> referenceCounter)
        {
            _asyncKeyedLocker = asyncKeyedLocker;
            _referenceCounter = referenceCounter;
        }

        public void Dispose()
        {
            var semaphoreSlim = _referenceCounter.SemaphoreSlim;

            while (true)
            {
                if (Monitor.TryEnter(semaphoreSlim))
                {
                    break;
                }
            }

            var remainingConsumers = --_referenceCounter.ReferenceCount;

            if (remainingConsumers == 0)
            {
                _asyncKeyedLocker.SemaphoreSlims.TryRemove(_referenceCounter.Key, out _);
            }

            Monitor.Exit(semaphoreSlim);

            semaphoreSlim.Release();
        }
    }
}
