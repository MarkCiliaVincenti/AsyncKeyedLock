using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock
{
    /// <summary>
    /// Represents an <see cref="IDisposable"/> for AsyncNonKeyedLocker with timeouts.
    /// </summary>
    public readonly struct AsyncNonKeyedLockTimeoutReleaser : IDisposable
    {
        private readonly bool _enteredSemaphore;

        /// <summary>
        /// True if the timeout was reached, false if not.
        /// </summary>
        public readonly bool EnteredSemaphore => _enteredSemaphore;

        private readonly SemaphoreSlim _semaphoreSlim;

        /// <summary>
        /// The exposed <see cref="SemaphoreSlim"/> instance used to limit the number of threads that can access the lock concurrently.
        /// </summary>
        public readonly SemaphoreSlim SemaphoreSlim => _semaphoreSlim;

        internal AsyncNonKeyedLockTimeoutReleaser(SemaphoreSlim semaphoreSlim, bool enteredSemaphore)
        {
            _enteredSemaphore = enteredSemaphore;
            _semaphoreSlim = semaphoreSlim;
        }

        /// <summary>
        /// Releases the <see cref="SemaphoreSlim"/> object once, depending on whether or not the semaphore was entered.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
            if (_enteredSemaphore)
            {
                _semaphoreSlim.Release();
            }            
        }
    }
}