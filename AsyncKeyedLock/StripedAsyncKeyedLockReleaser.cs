using System;
using System.Threading;

namespace AsyncKeyedLock
{
    /// <summary>
    /// Represents an <see cref="IDisposable"/> for AsyncKeyedLock.
    /// </summary>
    public struct StripedAsyncKeyedLockReleaser : IDisposable
    {
        /// <summary>
        /// The exposed <see cref="System.Threading.SemaphoreSlim"/> instance used to limit the number of threads that can access the lock concurrently.
        /// </summary>
        public SemaphoreSlim SemaphoreSlim { get; internal set; }

        /// <summary>
        /// Releases the <see cref="System.Threading.SemaphoreSlim"/> object once.
        /// </summary>
        public void Dispose()
        {
            SemaphoreSlim.Release();
        }
    }
}
