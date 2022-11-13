using System;
using System.Threading;

namespace AsyncKeyedLock
{
    /// <summary>
    /// Represents an <see cref="IDisposable"/> for AsyncKeyedLock.
    /// </summary>
    public interface IAsyncKeyedLockReleaser<TKey> : IDisposable
    {
        /// <summary>
        /// The key used for locking.
        /// </summary>
        TKey Key { get; }
        /// <summary>
        /// The number of threads processing or waiting to process for the specific <see cref="Key"/>.
        /// </summary>
        int ReferenceCount { get; internal set; }
        /// <summary>
        /// The exposed <see cref="SemaphoreSlim"/> instance used to limit the number of threads that can access the lock concurrently.
        /// </summary>
        SemaphoreSlim SemaphoreSlim { get; }
        /// <summary>
        /// Releases the <see cref="SemaphoreSlim"/> object once.
        /// </summary>
        new void Dispose();
    }
}