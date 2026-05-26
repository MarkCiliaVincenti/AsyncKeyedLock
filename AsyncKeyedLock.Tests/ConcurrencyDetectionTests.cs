using Xunit;
using Xunit.Abstractions;

namespace AsyncKeyedLock.Tests;

/// <summary>
/// Simplified test to detect concurrency bugs in AsyncKeyedLocker.
/// Tests whether multiple threads can simultaneously enter a critical section protected by the same lock.
/// </summary>
public class ConcurrencyDetectionTest
{
    private readonly ITestOutputHelper _output;

    public ConcurrencyDetectionTest(ITestOutputHelper output)
    {
        _output = output;
    }
    /// <summary>
    /// Test AsyncKeyedLocker with pooling enabled (default) using async API.
    /// This should show the concurrency bug.
    /// </summary>
    [Fact]
    public Task DetectConcurrency_WithPooling() =>
        RunConcurrencyTest(
            poolingEnabled: true,
            taskCount: 50,
            iterationsPerTask: 10000,
            workload: async () => await Task.Yield());

    /// <summary>
    /// Test AsyncKeyedLocker with pooling disabled.
    /// This should NOT show the concurrency bug.
    /// </summary>
    [Fact]
    public Task DetectConcurrency_WithoutPooling() =>
        RunConcurrencyTest(
            poolingEnabled: false,
            taskCount: 50,
            iterationsPerTask: 10000,
            workload: async () => await Task.Yield());

    /// <summary>
    /// Synchronous version test with pooling enabled.
    /// Tests LockOrNull (non-async) method.
    /// </summary>
    [Fact]
    public Task DetectConcurrency_Synchronous_WithPooling() =>
        RunConcurrencyTest(
            poolingEnabled: true,
            taskCount: 50,
            iterationsPerTask: 10000,
            workload: () => { Thread.SpinWait(100); return Task.CompletedTask; });

    /// <summary>
    /// Test with longer-held locks to increase chance of detecting race conditions.
    /// </summary>
    //[Test]
    public Task DetectConcurrency_LongerLocks_WithPooling() =>
        RunConcurrencyTest(
            poolingEnabled: true,
            taskCount: 20,
            iterationsPerTask: 1000,
            workload: async () => await Task.Delay(1));

    private async Task RunConcurrencyTest(
        bool poolingEnabled,
        int taskCount,
        int iterationsPerTask,
        Func<Task> workload)
    {
        var lockKey = Guid.NewGuid();
        var locker = poolingEnabled
            ? new AsyncKeyedLocker<Guid>()
            : new AsyncKeyedLocker<Guid>(options => options.PoolSize = 0);

        var metrics = new ConcurrencyMetrics(_output);

        var tasks = Enumerable.Range(0, taskCount).Select(async _ =>
        {
            for (int i = 0; i < iterationsPerTask; i++)
            {
                using var @lock = await locker.LockOrNullAsync(lockKey, 0);

                if (@lock != null)
                {
                    metrics.EnterCriticalSection(i);
                    await workload();
                    metrics.ExitCriticalSection();
                }

                metrics.IncrementIteration();
            }
        });

        await Task.WhenAll(tasks);

        metrics.LogResults();
        metrics.AssertExclusiveAccess();
    }

    private class ConcurrencyMetrics
    {
        private readonly ITestOutputHelper _output;
        private int _concurrencyDetector;
        private int _maxConcurrentAccess;
        private int _totalIterations;
        private int _concurrencyViolations;

        public ConcurrencyMetrics(ITestOutputHelper output)
        {
            _output = output;
        }

        public void EnterCriticalSection(int iteration)
        {
            var currentCount = Interlocked.Increment(ref _concurrencyDetector);

            UpdateMaxConcurrentAccess(currentCount);

            if (currentCount > 1)
            {
                Interlocked.Increment(ref _concurrencyViolations);
                _output.WriteLine($"CONCURRENCY VIOLATION! {currentCount} threads in critical section at iteration {iteration}");
            }
        }

        public void ExitCriticalSection()
        {
            Interlocked.Decrement(ref _concurrencyDetector);
        }

        public void IncrementIteration()
        {
            Interlocked.Increment(ref _totalIterations);
        }

        public void LogResults()
        {
            _output.WriteLine($"Total iterations: {_totalIterations}");
            _output.WriteLine($"Max concurrent access seen: {_maxConcurrentAccess}");
            _output.WriteLine($"Total concurrency violations: {_concurrencyViolations}");
        }

        public void AssertExclusiveAccess()
        {
            Assert.Equal(1, _maxConcurrentAccess);
        }

        private void UpdateMaxConcurrentAccess(int currentCount)
        {
            int currentMax;
            do
            {
                currentMax = _maxConcurrentAccess;
                if (currentCount <= currentMax) break;
            } while (Interlocked.CompareExchange(ref _maxConcurrentAccess, currentCount, currentMax) != currentMax);
        }
    }
}
