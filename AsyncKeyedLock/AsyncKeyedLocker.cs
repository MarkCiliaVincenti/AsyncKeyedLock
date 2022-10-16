using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncKeyedLock
{
    /// <summary>
    /// AsyncKeyedLock class, adapted and improved from <see href="https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key/31194647#31194647">Stephen Cleary's solution</see>.
    /// </summary>
    public sealed class AsyncKeyedLocker : IAsyncKeyedLocker
    {
        private readonly Dictionary<object, ReferenceCounter<SemaphoreSlim>> _semaphoreSlims = new Dictionary<object, ReferenceCounter<SemaphoreSlim>>();
        internal Dictionary<object, ReferenceCounter<SemaphoreSlim>> SemaphoreSlims => _semaphoreSlims;

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

        private SemaphoreSlim GetOrAdd(object key)
        {
            ReferenceCounter<SemaphoreSlim> referenceCounter;
            lock (SemaphoreSlims)
            {
                if (SemaphoreSlims.TryGetValue(key, out referenceCounter))
                {
                    ++referenceCounter.ReferenceCount;
                }
                else
                {
                    referenceCounter = new ReferenceCounter<SemaphoreSlim>(new SemaphoreSlim(MaxCount));
                    SemaphoreSlims[key] = referenceCounter;
                }
            }
            return referenceCounter.Value;
        }

        /// <summary>
        /// Synchronously lock based on a key
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(object key)
        {
            GetOrAdd(key).Wait();
            return new Releaser(this, key);
        }

        /// <summary>
        /// Asynchronously lock based on a key
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(object key)
        {
            var semaphoreSlim = GetOrAdd(key);
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            return new Releaser(this, key);
        }

        /// <summary>
        /// Checks whether or not there is a thread making use of a keyed lock.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns><see langword="true"/> if the key is in use; otherwise, false.</returns>
        public bool IsInUse(object key)
        {
            lock (SemaphoreSlims)
            {
                return SemaphoreSlims.ContainsKey(key);
            }
        }

        /// <summary>
        /// Get the number of requests concurrently locked for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of requests.</returns>
        [Obsolete("This method should not longer be used as it is confusing with Semaphore terminology. Use <see cref=\"GetCurrentCount\"/> or <see cref=\"GetRemaningCount\"/> instead depending what you want to do.")]
        public int GetCount(object key)
        {
            return GetRemainingCount(key);
        }

        /// <summary>
        /// Get the number of requests concurrently locked for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of requests concurrently locked for a given key.</returns>
        public int GetRemainingCount(object key)
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
        public int GetCurrentCount(object key)
        {
            return MaxCount - GetRemainingCount(key);
        }

        /// <summary>
        /// Forces requests to be released from the semaphore.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns><see langword="true"/> if the key is successfully found and removed; otherwise, false.</returns>
        public bool ForceRelease(object key)
        {
            lock (SemaphoreSlims)
            {
                return SemaphoreSlims.Remove(key);
            }
        }
    }
}
