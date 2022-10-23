using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncKeyedLock
{
    /// <summary>
    /// AsyncKeyedLock class, inspired by <see href="https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key/31194647#31194647">Stephen Cleary's solution</see>.
    /// </summary>
    public class AsyncKeyedLocker : AsyncKeyedLocker<object>, IAsyncKeyedLocker
    {
        /// <summary>
        /// Constructor for AsyncKeyedLock.
        /// </summary>
        public AsyncKeyedLocker()
        {
        }

        /// <summary>
        /// Constructor for AsyncKeyedLock.
        /// </summary>
        /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.</param>
        public AsyncKeyedLocker(int maxCount)
        {
            MaxCount = maxCount;
        }
    }

    /// <summary>
    /// AsyncKeyedLock class, adapted and improved from <see href="https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key/31194647#31194647">Stephen Cleary's solution</see>.
    /// </summary>
    public class AsyncKeyedLocker<TKey> : IAsyncKeyedLocker<TKey>
    {
        private readonly AsyncKeyedLockerDictionary<TKey> _semaphoreSlims;
        internal AsyncKeyedLockerDictionary<TKey> SemaphoreSlims => _semaphoreSlims;

        /// <summary>
        /// The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.
        /// </summary>
        public int MaxCount { get; set; } = 1;

        /// <summary>
        /// Constructor for AsyncKeyedLock.
        /// </summary>
        public AsyncKeyedLocker()
        {
            _semaphoreSlims = new AsyncKeyedLockerDictionary<TKey>(1);
        }

        /// <summary>
        /// Constructor for AsyncKeyedLock.
        /// </summary>
        /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.</param>
        public AsyncKeyedLocker(int maxCount)
        {
            MaxCount = maxCount;
            _semaphoreSlims = new AsyncKeyedLockerDictionary<TKey>(maxCount);
        }

        /// <summary>
        /// Synchronously lock based on a key.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            referenceCounter.SemaphoreSlim.WaitAsync();
            return referenceCounter.Releaser;
        }

        /// <summary>
        /// Synchronously lock based on a key, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, CancellationToken cancellationToken)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            referenceCounter.SemaphoreSlim.WaitAsync(cancellationToken);
            return referenceCounter.Releaser;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, int millisecondsTimeout)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            referenceCounter.SemaphoreSlim.Wait(millisecondsTimeout);
            return referenceCounter.Releaser;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, TimeSpan timeout)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            referenceCounter.SemaphoreSlim.Wait(timeout);
            return referenceCounter.Releaser;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            referenceCounter.SemaphoreSlim.Wait(millisecondsTimeout, cancellationToken);
            return referenceCounter.Releaser;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            referenceCounter.SemaphoreSlim.Wait(timeout, cancellationToken);
            return referenceCounter.Releaser;
        }

        /// <summary>
        /// Asynchronously lock based on a key.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            await referenceCounter.SemaphoreSlim.WaitAsync().ConfigureAwait(false);
            return referenceCounter.Releaser;
        }

        /// <summary>
        /// Asynchronously lock based on a key, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key, CancellationToken cancellationToken)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            await referenceCounter.SemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
            return referenceCounter.Releaser;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key, int millisecondsTimeout)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            await referenceCounter.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(false);
            return referenceCounter.Releaser;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key, TimeSpan timeout)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            await referenceCounter.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(false);
            return referenceCounter.Releaser;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            await referenceCounter.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(false);
            return referenceCounter.Releaser;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            await referenceCounter.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
            return referenceCounter.Releaser;
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
