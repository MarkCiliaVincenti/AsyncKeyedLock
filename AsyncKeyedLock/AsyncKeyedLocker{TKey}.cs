using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncKeyedLock
{
    /// <summary>
    /// AsyncKeyedLock class, adapted and improved from <see href="https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key/31194647#31194647">Stephen Cleary's solution</see>.
    /// </summary>
    public class AsyncKeyedLocker<TKey> : IAsyncKeyedLocker<TKey>
    {
        private readonly AsyncKeyedLockerDictionary<TKey> _semaphoreSlims = new AsyncKeyedLockerDictionary<TKey>();
        internal AsyncKeyedLockerDictionary<TKey> SemaphoreSlims => _semaphoreSlims;

        /// <summary>
        /// The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.
        /// </summary>
        public int MaxCount { get; set; }

        /// <summary>
        /// Constructor for AsyncKeyedLock.
        /// </summary>
        /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.</param>
        public AsyncKeyedLocker(int maxCount = 1)
        {
            MaxCount = maxCount;
        }

        private ReferenceCounter<TKey> GetOrAdd(TKey key)
        {
            return SemaphoreSlims.GetOrAdd(key, MaxCount);
        }

        /// <summary>
        /// Synchronously lock based on a key
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key)
        {
            var referenceCounter = GetOrAdd(key);
            referenceCounter.SemaphoreSlim.Wait();
            return new Releaser<TKey>(this, referenceCounter);
        }

        /// <summary>
        /// Asynchronously lock based on a key
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key)
        {
            var referenceCounter = GetOrAdd(key);
            await referenceCounter.SemaphoreSlim.WaitAsync().ConfigureAwait(false);
            return new Releaser<TKey>(this, referenceCounter);
        }

        /// <summary>
        /// Checks whether or not there is a thread making use of a keyed lock.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns><see langword="true"/> if the key is in use; otherwise, false.</returns>
        public bool IsInUse(TKey key)
        {
            return SemaphoreSlims.ContainsKey(key);
        }

        /// <summary>
        /// Get the number of requests concurrently locked for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of requests.</returns>
        [Obsolete("This method should not longer be used as it is confusing with Semaphore terminology. Use <see cref=\"GetCurrentCount\"/> or <see cref=\"GetRemaningCount\"/> instead depending what you want to do.")]
        public int GetCount(TKey key)
        {
            return GetRemainingCount(key);
        }

        /// <summary>
        /// Get the number of requests concurrently locked for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of requests concurrently locked for a given key.</returns>
        public int GetRemainingCount(TKey key)
        {
            lock (SemaphoreSlims)
            {
                if (SemaphoreSlims.TryGetValue(key, out var referenceCounter))
                {
                    return referenceCounter.ReferenceCount;
                }
                return 0;
            }
        }

        /// <summary>
        /// Get the number of remaining threads that can enter the lock for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of remaining threads that can enter the lock for a given key.</returns>
        public int GetCurrentCount(TKey key)
        {
            return MaxCount - GetRemainingCount(key);
        }

        /// <summary>
        /// Forces requests to be released from the semaphore.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns><see langword="true"/> if the key is successfully found and removed; otherwise, false.</returns>
        public bool ForceRelease(TKey key)
        {
            lock (SemaphoreSlims)
            {
                if (SemaphoreSlims.TryGetValue(key, out var referenceCounter))
                {
                    referenceCounter.SemaphoreSlim.Release(referenceCounter.ReferenceCount);
                    return SemaphoreSlims.TryRemove(key, out _);
                }
                return false;
            }
        }
    }
}
