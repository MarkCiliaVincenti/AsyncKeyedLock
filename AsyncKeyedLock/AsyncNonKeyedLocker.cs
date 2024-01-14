using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncKeyedLock
{
    /// <summary>
    /// Represents a lock, limiting concurrent threads to a specified number.
    /// </summary>
    public sealed class AsyncNonKeyedLocker : IDisposable
    {
        private readonly int _maxCount;

        /// <summary>
        /// The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.
        /// </summary>
        public int MaxCount => _maxCount;

        private SemaphoreSlim _semaphoreSlim;

        /// <summary>
        /// The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.
        /// </summary>
        public AsyncNonKeyedLocker(int maxCount = 1)
        {
            _maxCount = maxCount;
            _semaphoreSlim = new SemaphoreSlim(maxCount, maxCount);
        }

        #region Synchronous
        /// <summary>
        /// Synchronously lock.
        /// </summary>
        /// <returns>A disposable value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Lock()
        {
            _semaphoreSlim.Wait();
            return new AsyncNonKeyedLockReleaser(_semaphoreSlim);
        }

        /// <summary>
        /// Synchronously lock, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Lock(CancellationToken cancellationToken)
        {
            _semaphoreSlim.Wait(cancellationToken);
            return new AsyncNonKeyedLockReleaser(_semaphoreSlim);
        }

        /// <summary>
        /// Synchronously lock, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="entered">An out parameter showing whether or not the semaphore was entered.</param>
        /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncNonKeyedLockTimeoutReleaser Lock(int millisecondsTimeout, out bool entered)
        {
            entered = _semaphoreSlim.Wait(millisecondsTimeout);
            return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, entered);
        }

        /// <summary>
        /// Synchronously lock, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="entered">An out parameter showing whether or not the semaphore was entered.</param>
        /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncNonKeyedLockTimeoutReleaser Lock(TimeSpan timeout, out bool entered)
        {
            entered = _semaphoreSlim.Wait(timeout);
            return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, entered);
        }

        /// <summary>
        /// Synchronously lock, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="entered">An out parameter showing whether or not the semaphore was entered.</param>
        /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncNonKeyedLockTimeoutReleaser Lock(int millisecondsTimeout, CancellationToken cancellationToken, out bool entered)
        {
            try
            {
                entered = _semaphoreSlim.Wait(millisecondsTimeout, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                entered = false;
                throw;
            }
            return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, entered);
        }

        /// <summary>
        /// Synchronously lock, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="entered">An out parameter showing whether or not the semaphore was entered.</param>
        /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncNonKeyedLockTimeoutReleaser Lock(TimeSpan timeout, CancellationToken cancellationToken, out bool entered)
        {
            try
            {
                entered = _semaphoreSlim.Wait(timeout, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                entered = false;
                throw;
            }
            return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, entered);
        }
        #endregion Synchronous

        #region Asynchronous
        /// <summary>
        /// Asynchronously lock.
        /// </summary>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>A disposable value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<IDisposable> LockAsync(bool continueOnCapturedContext = false)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(continueOnCapturedContext);
            return new AsyncNonKeyedLockReleaser(_semaphoreSlim);
        }

        /// <summary>
        /// Asynchronously lock, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>A disposable value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<IDisposable> LockAsync(CancellationToken cancellationToken, bool continueOnCapturedContext = false)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
            return new AsyncNonKeyedLockReleaser(_semaphoreSlim);
        }

        /// <summary>
        /// Asynchronously lock, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(int millisecondsTimeout, bool continueOnCapturedContext = false)
        {
            bool entered = await _semaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(continueOnCapturedContext);
            return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, entered);
        }

        /// <summary>
        /// Asynchronously lock, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(TimeSpan timeout, bool continueOnCapturedContext = false)
        {
            bool entered = await _semaphoreSlim.WaitAsync(timeout).ConfigureAwait(continueOnCapturedContext);
            return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, entered);
        }

        /// <summary>
        /// Asynchronously lock, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(int millisecondsTimeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
        {
            try
            {
                bool entered = await _semaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(continueOnCapturedContext);
                return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, entered);
            }
            catch (OperationCanceledException)
            {
                return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, false);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously lock, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(TimeSpan timeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
        {
            try
            {
                bool entered = await _semaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(continueOnCapturedContext);
                return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, entered);
            }
            catch (OperationCanceledException)
            {
                return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, false);
                throw;
            }
        }
        #endregion Asynchronous

        #region AsynchronousNet8.0
#if NET8_0_OR_GREATER
        /// <summary>
        /// Asynchronously lock.
        /// </summary>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>A disposable value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<IDisposable> LockAsync(ConfigureAwaitOptions configureAwaitOptions)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(configureAwaitOptions);
            return new AsyncNonKeyedLockReleaser(_semaphoreSlim);
        }

        /// <summary>
        /// Asynchronously lock, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>A disposable value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<IDisposable> LockAsync(CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(configureAwaitOptions);
            return new AsyncNonKeyedLockReleaser(_semaphoreSlim);
        }

        /// <summary>
        /// Asynchronously lock, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(int millisecondsTimeout, ConfigureAwaitOptions configureAwaitOptions)
        {
            bool entered = await _semaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(configureAwaitOptions);
            return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, entered);
        }

        /// <summary>
        /// Asynchronously lock, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(TimeSpan timeout, ConfigureAwaitOptions configureAwaitOptions)
        {
            bool entered = await _semaphoreSlim.WaitAsync(timeout).ConfigureAwait(configureAwaitOptions);
            return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, entered);
        }

        /// <summary>
        /// Asynchronously lock, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(int millisecondsTimeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
        {
            try
            {
                bool entered = await _semaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(configureAwaitOptions);
                return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, entered);
            }
            catch (OperationCanceledException)
            {
                return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, false);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously lock, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(TimeSpan timeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
        {
            try
            {
                bool entered = await _semaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(configureAwaitOptions);
                return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, entered);
            }
            catch (OperationCanceledException)
            {
                return new AsyncNonKeyedLockTimeoutReleaser(_semaphoreSlim, false);
                throw;
            }
        }
#endif
        #endregion AsynchronousNet8.0

        /// <summary>
        /// Get the number of requests concurrently locked.
        /// </summary>
        /// <returns>The number of requests concurrently locked.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRemainingCount()
        {
            return _maxCount - _semaphoreSlim.CurrentCount;
        }

        /// <summary>
        /// Get the number of remaining threads that can enter the lock.
        /// </summary>
        /// <returns>The number of remaining threads that can enter the lock.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCurrentCount()
        {
            return _semaphoreSlim.CurrentCount;
        }

        /// <summary>
        /// Disposes the AsyncNonKeyedLocker.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _semaphoreSlim.Dispose();
        }

    }
}