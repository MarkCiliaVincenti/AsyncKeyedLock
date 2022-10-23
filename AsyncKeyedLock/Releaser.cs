using System;
using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class Releaser<TKey> : IDisposable
    {
        private readonly AsyncKeyedLockerDictionary<TKey> _asyncKeyedLockerDictionary;
        private readonly ReferenceCounter<TKey> _referenceCounter;

        public Releaser(AsyncKeyedLockerDictionary<TKey> asyncKeyedLockerDictionary, ReferenceCounter<TKey> referenceCounter)
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
