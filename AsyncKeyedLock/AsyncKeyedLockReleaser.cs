using System;
using System.Threading;

namespace AsyncKeyedLock
{
    /// <summary>
    /// AsyncKeyedLock releaser class
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public sealed class AsyncKeyedLockReleaser<TKey> : IDisposable
    {
        private readonly AsyncKeyedLockerDictionary<TKey> _asyncKeyedLockerDictionary;
        private readonly AsyncKeyedLockReferenceCounter<TKey> _referenceCounter;

        /// <summary>
        /// Constructor for AsyncKeyedLock releaser.
        /// </summary>
        /// <param name="asyncKeyedLockerDictionary">The dictionary instance.</param>
        /// <param name="referenceCounter">The reference counter instance.</param>
        public AsyncKeyedLockReleaser(AsyncKeyedLockerDictionary<TKey> asyncKeyedLockerDictionary, AsyncKeyedLockReferenceCounter<TKey> referenceCounter)
        {
            _asyncKeyedLockerDictionary = asyncKeyedLockerDictionary;
            _referenceCounter = referenceCounter;
        }

        /// <summary>
        /// Dispose and release.
        /// </summary>     
        public void Dispose()
        {
            _asyncKeyedLockerDictionary.Release(_referenceCounter);
        }
    }
}
