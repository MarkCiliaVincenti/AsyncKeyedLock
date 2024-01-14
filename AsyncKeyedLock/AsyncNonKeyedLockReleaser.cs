using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock
{
    /// <summary>
    /// Represents an <see cref="IDisposable"/> for AsyncNonKeyedLocker.
    /// </summary>
    public readonly struct AsyncNonKeyedLockReleaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphoreSlim;

        /// <summary>
        /// The exposed <see cref="SemaphoreSlim"/> instance used to limit the number of threads that can access the lock concurrently.
        /// </summary>
        public readonly SemaphoreSlim SemaphoreSlim => _semaphoreSlim;

        internal AsyncNonKeyedLockReleaser(SemaphoreSlim semaphoreSlim)
        {
            _semaphoreSlim = semaphoreSlim;
        }

        /// <summary>
        /// Releases the <see cref="SemaphoreSlim"/> object once.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
            _semaphoreSlim.Release();
        }
    }
}