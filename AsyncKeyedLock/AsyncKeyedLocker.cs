using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncKeyedLock
{
    /// <summary>
    /// AsyncKeyedLock class, adapted from <see href="https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key/31194647#31194647">Stephen Cleary's solution</see>.
    /// </summary>
    public sealed class AsyncKeyedLocker
    {
        private static readonly Dictionary<object, ReferenceCounter<SemaphoreSlim>> _semaphoreSlims = new Dictionary<object, ReferenceCounter<SemaphoreSlim>>();
        internal static Dictionary<object, ReferenceCounter<SemaphoreSlim>> SemaphoreSlims => _semaphoreSlims;

        private static SemaphoreSlim GetOrAdd(object key)
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
                    referenceCounter = new ReferenceCounter<SemaphoreSlim>(new SemaphoreSlim(1, 1));
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
        public static IDisposable Lock(object key)
        {
            GetOrAdd(key).Wait();
            return new Releaser(key);
        }

        /// <summary>
        /// Asynchronously lock based on a key
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        public static async Task<IDisposable> LockAsync(object key)
        {
            await GetOrAdd(key).WaitAsync().ConfigureAwait(false);
            return new Releaser(key);
        }
    }
}
