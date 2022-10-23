using System;
using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class AsyncKeyedLockReleaser<TKey> : IDisposable
    {
        private readonly AsyncKeyedLockerDictionary<TKey> _asyncKeyedLockerDictionary;
        private readonly AsyncKeyedLockReferenceCounter<TKey> _referenceCounter;

        public AsyncKeyedLockReleaser(AsyncKeyedLockerDictionary<TKey> asyncKeyedLockerDictionary, AsyncKeyedLockReferenceCounter<TKey> referenceCounter)
        {
            _asyncKeyedLockerDictionary = asyncKeyedLockerDictionary;
            _referenceCounter = referenceCounter;
        }

        public void Dispose()
        {
            _asyncKeyedLockerDictionary.Release(_referenceCounter);
        }
    }
}
