using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("AsyncKeyedLock.Tests, PublicKey=002400000480000094000000060200000024000052534131000" +
    "4000001000100a5cffbe51901ba498a225214c7eee4ff5f0341aad9f7605a596e72dbffdf234bcf2c157f7e3a4e2a3900cbc0d3919a6b" +
    "938cdf09e2aa5949fdd8f1dbda151853a00a08578724fb36f8c44112dadf388f75a5aab469f51a43b49f2e2fce355357291b01471606b" +
    "0c071fd5fe1641f1c7b0165d16f365748a613671681938cf6c1")]
namespace AsyncKeyedLock
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public sealed class StripedAsyncKeyedLocker<TKey> where TKey : notnull
    {
        /// <summary>
        /// The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.
        /// </summary>
        public int MaxCount { get; private set; }
        private readonly StripedAsyncKeyedLockReleaser[] _releasers;
        private readonly IEqualityComparer<TKey> _comparer;
        private readonly int _numberOfStripes;
        private static readonly EmptyDisposable _emptyDisposable = new EmptyDisposable();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfStripes"></param>
        /// <param name="maxCount"></param>
        /// <param name="comparer"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StripedAsyncKeyedLocker(int numberOfStripes = 4049, int maxCount = 1, IEqualityComparer<TKey> comparer = null)
        {
            MaxCount = maxCount;
            _numberOfStripes = HashHelpers.GetPrime(numberOfStripes);
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
            _releasers = new StripedAsyncKeyedLockReleaser[_numberOfStripes];
            for (int i = 0; i < _numberOfStripes; ++i)
            {
                _releasers[i] = new StripedAsyncKeyedLockReleaser
                {
                    SemaphoreSlim = new SemaphoreSlim(maxCount, maxCount)
                };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StripedAsyncKeyedLockReleaser Get(TKey key)
        {
            return _releasers[(_comparer.GetHashCode(key) & int.MaxValue) % _numberOfStripes];
        }

        #region Synchronous
        /// <summary>
        /// Synchronously lock based on a key.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key)
        {
            var releaser = Get(key);
            releaser.SemaphoreSlim.Wait();
            return releaser;
        }

        /// <summary>
        /// Synchronously lock based on a key, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, CancellationToken cancellationToken)
        {
            var releaser = Get(key);
            releaser.SemaphoreSlim.Wait(cancellationToken);
            return releaser;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="entered">An out parameter showing whether or not the semaphore was entered.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, int millisecondsTimeout, out bool entered)
        {
            var releaser = Get(key);
            if (millisecondsTimeout == Timeout.Infinite)
            {
                entered = true;
                releaser.SemaphoreSlim.Wait();
                return releaser;
            }
            entered = releaser.SemaphoreSlim.Wait(millisecondsTimeout);
            if (entered)
            {
                return new StripedAsyncKeyedLockTimeoutReleaser(true, releaser);
            }
            return _emptyDisposable;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="entered">An out parameter showing whether or not the semaphore was entered.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, TimeSpan timeout, out bool entered)
        {
            var releaser = Get(key);
            if (timeout.TotalMilliseconds == Timeout.Infinite)
            {
                entered = true;
                releaser.SemaphoreSlim.Wait();
                return releaser;
            }
            entered = releaser.SemaphoreSlim.Wait(timeout);
            if (entered)
            {
                return new StripedAsyncKeyedLockTimeoutReleaser(true, releaser);
            }
            return _emptyDisposable;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="entered">An out parameter showing whether or not the semaphore was entered.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, int millisecondsTimeout, CancellationToken cancellationToken, out bool entered)
        {
            var releaser = Get(key);
            try
            {
                if (millisecondsTimeout == Timeout.Infinite)
                {
                    entered = true;
                    releaser.SemaphoreSlim.Wait(cancellationToken);
                    return releaser;
                }
                entered = releaser.SemaphoreSlim.Wait(millisecondsTimeout, cancellationToken);
                if (entered)
                {
                    return new StripedAsyncKeyedLockTimeoutReleaser(true, releaser);
                }
                return _emptyDisposable;
            }
            catch (OperationCanceledException)
            {
                entered = false;
                throw;
            }
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="entered">An out parameter showing whether or not the semaphore was entered.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, TimeSpan timeout, CancellationToken cancellationToken, out bool entered)
        {
            var releaser = Get(key);
            try
            {
                if (timeout.TotalMilliseconds == Timeout.Infinite)
                {
                    entered = true;
                    releaser.SemaphoreSlim.Wait(cancellationToken);
                    return releaser;
                }
                entered = releaser.SemaphoreSlim.Wait(timeout, cancellationToken);
                if (entered)
                {
                    return new StripedAsyncKeyedLockTimeoutReleaser(true, releaser);
                }
                return _emptyDisposable;
            }
            catch (OperationCanceledException)
            {
                entered = false;
                throw;
            }
        }
        #endregion Synchronous

        #region SynchronousTry
        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public bool TryLock(TKey key, Action action, int millisecondsTimeout)
        {
            var releaser = Get(key);
            if (!releaser.SemaphoreSlim.Wait(millisecondsTimeout))
            {
                return false;
            }

            try
            {
                action();
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public bool TryLock(TKey key, Action action, TimeSpan timeout)
        {
            var releaser = Get(key);
            if (!releaser.SemaphoreSlim.Wait(timeout))
            {
                return false;
            }

            try
            {
                action();
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public bool TryLock(TKey key, Action action, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var releaser = Get(key);
            if (!releaser.SemaphoreSlim.Wait(millisecondsTimeout, cancellationToken))
            {
                return false;
            }

            try
            {
                action();
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public bool TryLock(TKey key, Action action, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var releaser = Get(key);
            if (!releaser.SemaphoreSlim.Wait(timeout, cancellationToken))
            {
                return false;
            }

            try
            {
                action();
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }
        #endregion SynchronousTry

        #region AsynchronousTry
        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Action action, int millisecondsTimeout, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(continueOnCapturedContext))
            {
                return false;
            }

            try
            {
                action();
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, int millisecondsTimeout, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(continueOnCapturedContext))
            {
                return false;
            }

            try
            {
                await task().ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Action action, TimeSpan timeout, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(continueOnCapturedContext))
            {
                return false;
            }

            try
            {
                action();
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, TimeSpan timeout, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(continueOnCapturedContext))
            {
                return false;
            }

            try
            {
                await task().ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Action action, int millisecondsTimeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                return false;
            }

            try
            {
                action();
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, int millisecondsTimeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                return false;
            }

            try
            {
                await task().ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Action action, TimeSpan timeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                return false;
            }

            try
            {
                action();
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, TimeSpan timeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                return false;
            }

            try
            {
                await task().ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }
        #endregion AsynchronousTry

        #region AsynchronousTryNet8.0
#if NET8_0_OR_GREATER
        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Action action, int millisecondsTimeout, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(configureAwaitOptions))
            {
                return false;
            }

            try
            {
                action();
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, int millisecondsTimeout, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(configureAwaitOptions))
            {
                return false;
            }

            try
            {
                await task().ConfigureAwait(configureAwaitOptions);
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Action action, TimeSpan timeout, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(configureAwaitOptions))
            {
                return false;
            }

            try
            {
                action();
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, TimeSpan timeout, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(configureAwaitOptions))
            {
                return false;
            }

            try
            {
                await task().ConfigureAwait(configureAwaitOptions);
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Action action, int millisecondsTimeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
            {
                return false;
            }

            try
            {
                action();
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, int millisecondsTimeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
            {
                return false;
            }

            try
            {
                await task().ConfigureAwait(configureAwaitOptions);
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Action action, TimeSpan timeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
            {
                return false;
            }

            try
            {
                action();
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, TimeSpan timeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            if (!await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
            {
                return false;
            }

            try
            {
                await task().ConfigureAwait(configureAwaitOptions);
            }
            finally
            {
                releaser.Dispose();
            }
            return true;
        }
#endif
        #endregion AsynchronousTryNet8.0

        #region Asynchronous
        /// <summary>
        /// Asynchronously lock based on a key.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>A disposable value.</returns>
        public async ValueTask<StripedAsyncKeyedLockReleaser> LockAsync(TKey key, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            await releaser.SemaphoreSlim.WaitAsync().ConfigureAwait(continueOnCapturedContext);
            return releaser;
        }

        /// <summary>
        /// Asynchronously lock based on a key, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>A disposable value.</returns>
        public async ValueTask<StripedAsyncKeyedLockReleaser> LockAsync(TKey key, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            await releaser.SemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
            return releaser;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>A disposable value of type <see cref="StripedAsyncKeyedLockTimeoutReleaser"/>.</returns>
        public async ValueTask<StripedAsyncKeyedLockTimeoutReleaser> LockAsync(TKey key, int millisecondsTimeout, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            return new StripedAsyncKeyedLockTimeoutReleaser(await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(continueOnCapturedContext), releaser);
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>A disposable value of type <see cref="StripedAsyncKeyedLockTimeoutReleaser"/>.</returns>
        public async ValueTask<StripedAsyncKeyedLockTimeoutReleaser> LockAsync(TKey key, TimeSpan timeout, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            return new StripedAsyncKeyedLockTimeoutReleaser(await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(continueOnCapturedContext), releaser);
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>A disposable value of type <see cref="StripedAsyncKeyedLockTimeoutReleaser"/>.</returns>
        public async ValueTask<StripedAsyncKeyedLockTimeoutReleaser> LockAsync(TKey key, int millisecondsTimeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            return new StripedAsyncKeyedLockTimeoutReleaser(await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(continueOnCapturedContext), releaser);
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
        /// <returns>A disposable value of type <see cref="StripedAsyncKeyedLockTimeoutReleaser"/>.</returns>
        public async ValueTask<StripedAsyncKeyedLockTimeoutReleaser> LockAsync(TKey key, TimeSpan timeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
        {
            var releaser = Get(key);
            return new StripedAsyncKeyedLockTimeoutReleaser(await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(continueOnCapturedContext), releaser);
        }
        #endregion Asynchronous

        #region AsynchronousNet8.0
#if NET8_0_OR_GREATER
        /// <summary>
        /// Asynchronously lock based on a key.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>A disposable value.</returns>
        public async ValueTask<StripedAsyncKeyedLockReleaser> LockAsync(TKey key, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            await releaser.SemaphoreSlim.WaitAsync().ConfigureAwait(configureAwaitOptions);
            return releaser;
        }

        /// <summary>
        /// Asynchronously lock based on a key, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>A disposable value.</returns>
        public async ValueTask<StripedAsyncKeyedLockReleaser> LockAsync(TKey key, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            await releaser.SemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(configureAwaitOptions);
            return releaser;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>A disposable value of type <see cref="StripedAsyncKeyedLockTimeoutReleaser"/>.</returns>
        public async ValueTask<StripedAsyncKeyedLockTimeoutReleaser> LockAsync(TKey key, int millisecondsTimeout, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            return new StripedAsyncKeyedLockTimeoutReleaser(await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(configureAwaitOptions), releaser);
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>A disposable value of type <see cref="StripedAsyncKeyedLockTimeoutReleaser"/>.</returns>
        public async ValueTask<StripedAsyncKeyedLockTimeoutReleaser> LockAsync(TKey key, TimeSpan timeout, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            return new StripedAsyncKeyedLockTimeoutReleaser(await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(configureAwaitOptions), releaser);
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>A disposable value of type <see cref="StripedAsyncKeyedLockTimeoutReleaser"/>.</returns>
        public async ValueTask<StripedAsyncKeyedLockTimeoutReleaser> LockAsync(TKey key, int millisecondsTimeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            return new StripedAsyncKeyedLockTimeoutReleaser(await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(configureAwaitOptions), releaser);
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
        /// <returns>A disposable value of type <see cref="StripedAsyncKeyedLockTimeoutReleaser"/>.</returns>
        public async ValueTask<StripedAsyncKeyedLockTimeoutReleaser> LockAsync(TKey key, TimeSpan timeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
        {
            var releaser = Get(key);
            return new StripedAsyncKeyedLockTimeoutReleaser(await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(configureAwaitOptions), releaser);
        }
#endif
        #endregion AsynchronousNet8.0

        /// <summary>
        /// Checks whether or not there is a thread making use of a keyed lock. Since striped locking means some keys could share the same lock,
        /// a value of true does not necessarily mean that the key is in use but that its lock is in use.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns><see langword="true"/> if the key's lock is in use; otherwise, false.</returns>
        public bool IsInUse(TKey key)
        {
            return Get(key).SemaphoreSlim.CurrentCount < MaxCount;
        }
    }
}
