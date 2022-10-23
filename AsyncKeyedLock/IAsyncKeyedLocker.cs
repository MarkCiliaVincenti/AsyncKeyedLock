using System;
using System.Threading.Tasks;

namespace AsyncKeyedLock
{
    /// <summary>
    /// AsyncKeyedLock interface
    /// </summary>
    public interface IAsyncKeyedLocker<TKey>
    {
        /// <summary>
        /// The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.
        /// </summary>
        int MaxCount { get; set; }

        /// <summary>
        /// Synchronously lock based on a key
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        IDisposable Lock(TKey key);

        /// <summary>
        /// Asynchronously lock based on a key
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        Task<IDisposable> LockAsync(TKey key);

        /// <summary>
        /// Checks whether or not there is a thread making use of a keyed lock.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns><see langword="true"/> if the key is in use; otherwise, false.</returns>
        bool IsInUse(TKey key);

        /// <summary>
        /// Get the number of requests concurrently locked for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of requests.</returns>
        [Obsolete("This method should not longer be used as it is confusing with Semaphore terminology. Use <see cref=\"GetCurrentCount\"/> or <see cref=\"GetRemaningCount\"/> instead depending what you want to do.")]
        int GetCount(TKey key);

        /// <summary>
        /// Get the number of requests concurrently locked for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of requests concurrently locked for a given key.</returns>
        int GetRemainingCount(TKey key);

        /// <summary>
        /// Get the number of remaining threads that can enter the lock for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of remaining threads that can enter the lock for a given key.</returns>
        int GetCurrentCount(TKey key);

        /// <summary>
        /// Forces requests to be released from the semaphore.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns><see langword="true"/> if the key is successfully found and removed; otherwise, false.</returns>
        bool ForceRelease(TKey key);
    }
}
