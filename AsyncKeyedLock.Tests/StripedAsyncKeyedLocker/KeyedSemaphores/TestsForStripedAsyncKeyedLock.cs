using FluentAssertions;
using System.Collections.Concurrent;
using Xunit;
using Xunit.Abstractions;

namespace AsyncKeyedLock.Tests.StripedAsyncKeyedLocker.KeyedSemaphores;

/// <summary>
/// Adapted from https://raw.githubusercontent.com/amoerie/keyed-semaphores/main/KeyedSemaphores.Tests/TestsForKeyedSemaphore.cs and https://raw.githubusercontent.com/amoerie/keyed-semaphores/main/KeyedSemaphores.Tests/TestsForKeyedSemaphoresCollection.cs
/// </summary>
public class TestsForStripedAsyncKeyedLock
{
    private readonly ITestOutputHelper _output;
    private readonly StripedAsyncKeyedLocker<string> _keyedLocker = new();
    TimeSpan _defaultSynchronousWaitDuration = TimeSpan.FromMilliseconds(10);

    public TestsForStripedAsyncKeyedLock(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public class Async : TestsForStripedAsyncKeyedLock
    {
        public Async(ITestOutputHelper output) : base(output) { }

        [Theory]
        [InlineData(100, 100, 6, 100)]
        [InlineData(100, 10, 2, 10)]
        [InlineData(100, 50, 5, 50)]
        [InlineData(100, 1, 1, 1)]
        public async Task ShouldApplyParallelismCorrectly(int numberOfThreads, int numberOfKeys, int minParallelism,
            int maxParallelism)
        {
            // Arrange
            var runningTasksIndex = new ConcurrentDictionary<int, int>();
            var parallelismLock = new object();
            var currentParallelism = 0;
            var peakParallelism = 0;

            var threads = Enumerable.Range(0, numberOfThreads)
                .Select(i =>
                    Task.Run(async () => await OccupyTheLockALittleBit(i % numberOfKeys)))
                .ToList();

            // Act + Assert
            await Task.WhenAll(threads);

            peakParallelism.Should().BeLessOrEqualTo(maxParallelism);
            peakParallelism.Should().BeGreaterOrEqualTo(minParallelism);

            _output.WriteLine("Peak parallelism was " + peakParallelism);

            async Task OccupyTheLockALittleBit(int key)
            {
                using (await _keyedLocker.LockAsync(key.ToString()))
                {
                    var incrementedCurrentParallelism = Interlocked.Increment(ref currentParallelism);

                    lock (parallelismLock)
                    {
                        peakParallelism = Math.Max(incrementedCurrentParallelism, peakParallelism);
                    }

                    var currentTaskId = Task.CurrentId ?? -1;

                    if (!runningTasksIndex.TryAdd(key, currentTaskId))
                    {
                        throw new InvalidOperationException(
                            $"Task #{currentTaskId} acquired a lock using key ${key} but another thread is also still running using this key!");
                    }

                    const int delay = 10;

                    await Task.Delay(delay);

                    if (!runningTasksIndex.TryRemove(key, out var value))
                    {
                        throw new InvalidOperationException($"Task #{currentTaskId} has just finished " +
                                                            $"but the running tasks index does not contain an entry for key {key}");
                    }

                    if (value != currentTaskId)
                    {
                        var ex = new InvalidOperationException($"Task #{currentTaskId} has just finished " +
                                                               $"but the running threads index has linked task #{value} to key {key}!");

                        throw ex;
                    }

                    Interlocked.Decrement(ref currentParallelism);
                }
            }
        }
    }

    public class Sync : TestsForStripedAsyncKeyedLock
    {
        public Sync(ITestOutputHelper output) : base(output) { }

        [Theory]
        [InlineData(100, 100, 10, 100)]
        [InlineData(100, 10, 2, 10)]
        [InlineData(100, 50, 5, 50)]
        [InlineData(100, 1, 1, 1)]
        public void ShouldApplyParallelismCorrectly(int numberOfThreads, int numberOfKeys, int minParallelism,
            int maxParallelism)
        {
            // Arrange
            var currentParallelism = 0;
            var peakParallelism = 0;
            var parallelismLock = new object();
            var runningThreadsIndex = new ConcurrentDictionary<int, int>();

            var threads = Enumerable.Range(0, numberOfThreads)
                .Select(i => new Thread(() => OccupyTheLockALittleBit(i % numberOfKeys)))
                .ToList();

            // Act
            foreach (var thread in threads) thread.Start();

            foreach (var thread in threads) thread.Join();

            peakParallelism.Should().BeGreaterThanOrEqualTo(minParallelism);
            peakParallelism.Should().BeLessThanOrEqualTo(maxParallelism);

            _output.WriteLine("Peak parallelism was " + peakParallelism);

            void OccupyTheLockALittleBit(int key)
            {
                using (_keyedLocker.Lock(key.ToString()))
                {
                    var incrementedCurrentParallelism = Interlocked.Increment(ref currentParallelism);

                    lock (parallelismLock)
                    {
                        peakParallelism = Math.Max(incrementedCurrentParallelism, peakParallelism);
                    }

                    var currentThreadId = Thread.CurrentThread.ManagedThreadId;

                    if (!runningThreadsIndex.TryAdd(key, currentThreadId))
                    {
                        throw new InvalidOperationException(
                            $"Thread #{currentThreadId} acquired a lock using key ${key} but another thread is also still running using this key!");
                    }

                    const int delay = 10;

                    Thread.Sleep(delay);

                    if (!runningThreadsIndex.TryRemove(key, out var value))
                    {
                        throw new InvalidOperationException($"Thread #{currentThreadId} has just finished " +
                                                            $"but the running threads index does not contain an entry for key {key}");
                    }

                    if (value != currentThreadId)
                    {
                        var ex = new InvalidOperationException($"Thread #{currentThreadId} has just finished " +
                                                               $"but the running threads index has linked thread #{value} to key {key}!");

                        throw ex;
                    }

                    Interlocked.Decrement(ref currentParallelism);
                }
            }
        }
    }

    [Fact]
    public async Task ThreeDifferentLocksShouldWork()
    {
        // Arrange
        var keyedLocker = new StripedAsyncKeyedLocker<int>();

        // Act
        using var _1 = await keyedLocker.LockAsync(1);
        using var _2 = await keyedLocker.LockAsync(2);
        using var _3 = await keyedLocker.LockAsync(3);

        // Assert
        _1.Should().NotBeNull();
        _2.Should().NotBeNull();
        _3.Should().NotBeNull();
    }

    [Fact]
    public async Task ThreeIdenticalLocksShouldWork()
    {
        // Arrange
        var keyedLocker = new StripedAsyncKeyedLocker<int>();

        // Act
        var t1 = Task.Run(async () =>
        {
            using var _ = await keyedLocker.LockAsync(1);
        });
        var t2 = Task.Run(async () =>
        {
            using var _ = await keyedLocker.LockAsync(1);
        });
        var t3 = Task.Run(async () =>
        {
            using var _ = await keyedLocker.LockAsync(1);
        });
        await t1;
        await t2;
        await t3;

        // Assert
        t1.Should().NotBeNull();
        t2.Should().NotBeNull();
        t3.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldRunThreadsWithDistinctKeysInParallel()
    {
        // Arrange
        var currentParallelism = 0;
        var maxParallelism = 0;
        var parallelismLock = new object();
        var keyedLocker = new StripedAsyncKeyedLocker<int>();

        // 100 threads, 100 keys
        var threads = Enumerable.Range(0, 100)
            .Select(i => Task.Run(async () => await OccupyTheLockALittleBit(i)))
            .ToList();

        // Act
        await Task.WhenAll(threads);

        maxParallelism.Should().BeGreaterThan(10);
        foreach (var key in Enumerable.Range(0, 100))
        {
            keyedLocker.IsInUse(key).Should().BeFalse();
        }

        async Task OccupyTheLockALittleBit(int key)
        {
            using (await keyedLocker.LockAsync(key))
            {
                var incrementedCurrentParallelism = Interlocked.Increment(ref currentParallelism);

                lock (parallelismLock)
                {
                    maxParallelism = Math.Max(incrementedCurrentParallelism, maxParallelism);
                }

                const int delay = 250;


                await Task.Delay(TimeSpan.FromMilliseconds(delay));

                Interlocked.Decrement(ref currentParallelism);
            }
        }
    }

    [Fact]
    public async Task ShouldRunThreadsWithSameKeysLinearly()
    {
        // Arrange
        var runningTasksIndex = new ConcurrentDictionary<int, int>();
        var parallelismLock = new object();
        var currentParallelism = 0;
        var maxParallelism = 0;
        var keyedLocker = new StripedAsyncKeyedLocker<int>();

        // 100 threads, 10 keys
        var threads = Enumerable.Range(0, 100)
            .Select(i => Task.Run(async () => await OccupyTheLockALittleBit(i % 10)))
            .ToList();

        // Act + Assert
        await Task.WhenAll(threads);

        maxParallelism.Should().BeLessOrEqualTo(10);
        foreach (var key in Enumerable.Range(0, 100))
        {
            keyedLocker.IsInUse(key % 10).Should().BeFalse();
        }

        async Task OccupyTheLockALittleBit(int key)
        {
            using (await keyedLocker.LockAsync(key))
            {
                var incrementedCurrentParallelism = Interlocked.Increment(ref currentParallelism);

                lock (parallelismLock)
                {
                    maxParallelism = Math.Max(incrementedCurrentParallelism, maxParallelism);
                }

                var currentTaskId = Task.CurrentId ?? -1;
                if (runningTasksIndex.TryGetValue(key, out var otherThread))
                    throw new Exception($"Thread #{currentTaskId} acquired a lock using key ${key} " +
                                        $"but another thread #{otherThread} is also still running using this key!");

                runningTasksIndex[key] = currentTaskId;

                const int delay = 10;

                await Task.Delay(TimeSpan.FromMilliseconds(delay));

                if (!runningTasksIndex.TryRemove(key, out var value))
                {
                    var ex = new Exception($"Thread #{currentTaskId} has finished " +
                                           "but when trying to cleanup the running threads index, the value is already gone");

                    throw ex;
                }

                if (value != currentTaskId)
                {
                    var ex = new Exception(
                        $"Thread #{currentTaskId} has finished and has removed itself from the running threads index," +
                        $" but that index contained an incorrect value: #{value}!");

                    throw ex;
                }

                Interlocked.Decrement(ref currentParallelism);
            }
        }
    }

    [Fact]
    public async Task ShouldNeverCreateTwoSemaphoresForTheSameKey()
    {
        // Arrange
        var runningTasksIndex = new ConcurrentDictionary<int, int>();
        var parallelismLock = new object();
        var currentParallelism = 0;
        var maxParallelism = 0;
        var random = new Random();
        var keyedLocker = new StripedAsyncKeyedLocker<int>();

        // Many threads, 1 key
        var threads = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(async () => await OccupyTheLockALittleBit(1)))
            .ToList();

        // Act + Assert
        await Task.WhenAll(threads);

        maxParallelism.Should().Be(1);
        keyedLocker.IsInUse(1).Should().BeFalse();


        async Task OccupyTheLockALittleBit(int key)
        {
            var currentTaskId = Task.CurrentId ?? -1;
            var delay = random.Next(500);

            await Task.Delay(delay);

            using (await keyedLocker.LockAsync(key))
            {
                var incrementedCurrentParallelism = Interlocked.Increment(ref currentParallelism);

                lock (parallelismLock)
                {
                    maxParallelism = Math.Max(incrementedCurrentParallelism, maxParallelism);
                }

                if (runningTasksIndex.TryGetValue(key, out var otherThread))
                    throw new Exception($"Task [{currentTaskId,3}] has a lock for key ${key} " +
                                        $"but another task [{otherThread,3}] also has an active lock for this key!");

                runningTasksIndex[key] = currentTaskId;

                if (!runningTasksIndex.TryRemove(key, out var value))
                {
                    var ex = new Exception($"Task [{currentTaskId,3}] has finished " +
                                           "but when trying to cleanup the running tasks index, the value is already gone");

                    throw ex;
                }

                if (value != currentTaskId)
                {
                    var ex = new Exception(
                        $"Task [{currentTaskId,3}] has finished and has removed itself from the running tasks index," +
                        $" but that index contained a task ID of another task: [{value}]!");

                    throw ex;
                }

                Interlocked.Decrement(ref currentParallelism);
            }
        }
    }

    [Fact]
    public async Task ShouldRunThreadsWithDistinctStringKeysInParallel()
    {
        // Arrange
        var currentParallelism = 0;
        var maxParallelism = 0;
        var parallelismLock = new object();
        var keyedLocker = new StripedAsyncKeyedLocker<int>();

        // 100 threads, 100 keys
        var threads = Enumerable.Range(0, 100)
            .Select(i => Task.Run(async () => await OccupyTheLockALittleBit(i)))
            .ToList();

        // Act
        await Task.WhenAll(threads);

        maxParallelism.Should().BeGreaterThan(10);
        foreach (var key in Enumerable.Range(0, 100))
        {
            keyedLocker.IsInUse(key).Should().BeFalse();
        }

        async Task OccupyTheLockALittleBit(int key)
        {
            using (await keyedLocker.LockAsync(key))
            {
                var incrementedCurrentParallelism = Interlocked.Increment(ref currentParallelism);

                lock (parallelismLock)
                {
                    maxParallelism = Math.Max(incrementedCurrentParallelism, maxParallelism);
                }

                const int delay = 250;

                await Task.Delay(TimeSpan.FromMilliseconds(delay));

                Interlocked.Decrement(ref currentParallelism);
            }
        }
    }

    [Fact]
    public async Task IsInUseShouldReturnTrueWhenLockedAndFalseWhenNotLocked()
    {
        // Arrange
        var keyedLocker = new StripedAsyncKeyedLocker<int>();

        // 10 threads, 10 keys
        var threads = Enumerable.Range(0, 10)
            .Select(i => Task.Run(async () => await OccupyTheLockALittleBit(i)))
            .ToList();

        // Act
        await Task.WhenAll(threads);
        foreach (var key in Enumerable.Range(0, 10))
        {
            keyedLocker.IsInUse(key).Should().BeFalse();
        }

        async Task OccupyTheLockALittleBit(int key)
        {
            keyedLocker.IsInUse(key).Should().BeFalse();

            using (await keyedLocker.LockAsync(key))
            {
                const int delay = 250;

                await Task.Delay(TimeSpan.FromMilliseconds(delay));

                keyedLocker.IsInUse(key).Should().BeTrue();
            }

            keyedLocker.IsInUse(key).Should().BeFalse();
        }
    }

    [Fact]
    public void Lock_WhenCancelled_ShouldReleaseKeyedSemaphoreAndThrowOperationCanceledException()
    {
        // Arrange
        var collection = new StripedAsyncKeyedLocker<string>();
        var cancelledCancellationToken = new CancellationToken(true);

        // Act
        Action action = () =>
        {
            using var _ = collection.Lock("test", cancelledCancellationToken);
        };
        action.Should().Throw<OperationCanceledException>();

        // Assert
        collection.IsInUse("test").Should().BeFalse();
    }

    [Fact]
    public void Lock_WhenNotCancelled_ShouldReturnDisposable()
    {
        // Arrange
        var collection = new StripedAsyncKeyedLocker<string>();
        var cancellationToken = default(CancellationToken);

        // Act
        var releaser = collection.Lock("test", cancellationToken);

        // Assert
        collection.IsInUse("test").Should().BeTrue();
        releaser.Dispose();
        collection.IsInUse("test").Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void
        TryLock_WhenCancelled_ShouldReleaseKeyedSemaphoreAndThrowOperationCanceledExceptionAndNotInvokeCallback(
            bool useShortTimeout)
    {
        // Arrange
        var isLockAcquired = false;
        var isCallbackInvoked = false;

        void Callback()
        {
            isCallbackInvoked = true;
        }

        var collection = new StripedAsyncKeyedLocker<string>();
        var cancelledCancellationToken = new CancellationToken(true);
        var timeout = useShortTimeout
            ? _defaultSynchronousWaitDuration.Subtract(TimeSpan.FromMilliseconds(1))
            : _defaultSynchronousWaitDuration.Add(TimeSpan.FromMilliseconds(1));

        // Act
        Action action = () =>
            isLockAcquired = collection.TryLock("test", Callback, timeout, cancelledCancellationToken);
        action.Should().Throw<OperationCanceledException>();

        action = () =>
            isLockAcquired = collection.TryLock("test", Callback, Convert.ToInt32(timeout.TotalMilliseconds), cancelledCancellationToken);
        action.Should().Throw<OperationCanceledException>();

        // Assert
        collection.IsInUse("test").Should().BeFalse();
        isLockAcquired.Should().BeFalse();
        isCallbackInvoked.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TryLock_WhenNotCancelled_ShouldInvokeCallbackAndReturnDisposable(bool useShortTimeout)
    {
        // Arrange
        bool isCallbackInvoked = false;

        void Callback()
        {
            isCallbackInvoked = true;
        }

        var collection = new StripedAsyncKeyedLocker<string>();
        var cancellationToken = default(CancellationToken);
        var timeout = useShortTimeout
            ? _defaultSynchronousWaitDuration.Subtract(TimeSpan.FromMilliseconds(1))
            : _defaultSynchronousWaitDuration.Add(TimeSpan.FromMilliseconds(1));

        // Act
        var isLockAcquired = collection.TryLock("test", Callback, timeout, cancellationToken);

        // Assert
        collection.IsInUse("test").Should().BeFalse();
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();

        isLockAcquired = collection.TryLock("test", Callback, Convert.ToInt32(timeout.TotalMilliseconds), cancellationToken);

        // Assert
        collection.IsInUse("test").Should().BeFalse();
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task LockAsync_WhenCancelled_ShouldReleaseKeyedSemaphoreAndThrowOperationCanceledException()
    {
        // Arrange
        var collection = new StripedAsyncKeyedLocker<string>();
        var cancelledCancellationToken = new CancellationToken(true);

        // Act
        Func<Task> action = async () =>
        {
            using var _ = await collection.LockAsync("test", cancelledCancellationToken);
        };
        await action.Should().ThrowAsync<OperationCanceledException>();

        // Assert
        collection.IsInUse("test").Should().BeFalse();
    }

    [Fact]
    public async Task LockAsync_WhenNotCancelled_ShouldReturnDisposable()
    {
        // Arrange
        var collection = new StripedAsyncKeyedLocker<string>();
        var cancellationToken = default(CancellationToken);

        // Act
        var releaser = await collection.LockAsync("test", cancellationToken);

        // Assert
        collection.IsInUse("test").Should().BeTrue();
        releaser.Dispose();
        collection.IsInUse("test").Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task
        TryLockAsync_WithSynchronousCallback_WhenCancelled_ShouldReleaseKeyedSemaphoreAndThrowOperationCanceledExceptionAndNotInvokeCallback(
            bool useShortTimeout)
    {
        // Arrange
        bool isLockAcquired = false;
        bool isCallbackInvoked = false;

        void Callback()
        {
            isCallbackInvoked = true;
        }

        var collection = new StripedAsyncKeyedLocker<string>();
        var cancelledCancellationToken = new CancellationToken(true);
        var timeout = useShortTimeout
            ? _defaultSynchronousWaitDuration.Subtract(TimeSpan.FromMilliseconds(1))
            : _defaultSynchronousWaitDuration.Add(TimeSpan.FromMilliseconds(1));

        // Act
        Func<Task> action = async () =>
            isLockAcquired = await collection.TryLockAsync("test", Callback, timeout, cancelledCancellationToken);
        await action.Should().ThrowAsync<OperationCanceledException>();

        action = async () =>
            isLockAcquired = await collection.TryLockAsync("test", Callback, Convert.ToInt32(timeout.TotalMilliseconds), cancelledCancellationToken);
        await action.Should().ThrowAsync<OperationCanceledException>();

        // Assert
        collection.IsInUse("test").Should().BeFalse();
        isLockAcquired.Should().BeFalse();
        isCallbackInvoked.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TryLockAsync_WithSynchronousCallback_WhenNotCancelled_ShouldInvokeCallbackAndReturnTrue(
        bool useShortTimeout)
    {
        // Arrange
        var isCallbackInvoked = false;

        void Callback()
        {
            isCallbackInvoked = true;
        }

        var collection = new StripedAsyncKeyedLocker<string>();
        var cancellationToken = default(CancellationToken);
        var timeout = useShortTimeout
            ? _defaultSynchronousWaitDuration.Subtract(TimeSpan.FromMilliseconds(1))
            : _defaultSynchronousWaitDuration.Add(TimeSpan.FromMilliseconds(1));

        // Act
        var isLockAcquired = await collection.TryLockAsync("test", Callback, timeout, cancellationToken);

        // Assert
        collection.IsInUse("test").Should().BeFalse();
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();

        isLockAcquired = await collection.TryLockAsync("test", Callback, Convert.ToInt32(timeout.TotalMilliseconds), cancellationToken);

        // Assert
        collection.IsInUse("test").Should().BeFalse();
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task
        TryLockAsync_WithAsynchronousCallback_WhenCancelled_ShouldReleaseKeyedSemaphoreAndThrowOperationCanceledExceptionAndNotInvokeCallback(
            bool useShortTimeout)
    {
        // Arrange
        bool isLockAcquired = false;
        bool isCallbackInvoked = false;

        async Task Callback()
        {
            await Task.Delay(1);
            isCallbackInvoked = true;
        }

        var collection = new StripedAsyncKeyedLocker<string>();
        var cancelledCancellationToken = new CancellationToken(true);
        var timeout = useShortTimeout
            ? _defaultSynchronousWaitDuration.Subtract(TimeSpan.FromMilliseconds(1))
            : _defaultSynchronousWaitDuration.Add(TimeSpan.FromMilliseconds(1));

        // Act
        Func<Task> action = async () =>
        {
            isLockAcquired =
                await collection.TryLockAsync("test", Callback, timeout, cancelledCancellationToken);
        };
        await action.Should().ThrowAsync<OperationCanceledException>();

        action = async () =>
        {
            isLockAcquired =
                await collection.TryLockAsync("test", Callback, Convert.ToInt32(timeout.TotalMilliseconds), cancelledCancellationToken);
        };
        await action.Should().ThrowAsync<OperationCanceledException>();

        // Assert
        collection.IsInUse("test").Should().BeFalse();
        isLockAcquired.Should().BeFalse();
        isCallbackInvoked.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TryLockAsync_WithAsynchronousCallback_WhenNotCancelled_ShouldInvokeCallbackAndReturnTrue(
        bool useShortTimeout)
    {
        // Arrange
        var isCallbackInvoked = false;

        async Task Callback()
        {
            await Task.Delay(1);
            isCallbackInvoked = true;
        }

        var collection = new StripedAsyncKeyedLocker<string>();
        var cancellationToken = default(CancellationToken);
        var timeout = useShortTimeout
            ? _defaultSynchronousWaitDuration.Subtract(TimeSpan.FromMilliseconds(1))
            : _defaultSynchronousWaitDuration.Add(TimeSpan.FromMilliseconds(1));

        // Act
        var isLockAcquired = await collection.TryLockAsync("test", Callback, timeout, cancellationToken);

        // Assert
        collection.IsInUse("test").Should().BeFalse();
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();

        isLockAcquired = await collection.TryLockAsync("test", Callback, Convert.ToInt32(timeout.TotalMilliseconds), cancellationToken);

        // Assert
        collection.IsInUse("test").Should().BeFalse();
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TryLock_WhenTimedOut_ShouldNotInvokeCallbackAndReturnFalse(bool useShortTimeout)
    {
        // Arrange
        var collection = new StripedAsyncKeyedLocker<string>();
        var key = "test";
        using var _ = collection.Lock(key);
        var isCallbackInvoked = false;

        void Callback()
        {
            isCallbackInvoked = true;
        }

        var timeout = useShortTimeout
            ? _defaultSynchronousWaitDuration.Subtract(TimeSpan.FromMilliseconds(1))
            : _defaultSynchronousWaitDuration.Add(TimeSpan.FromMilliseconds(1));

        // Act
        var isLockAcquired = collection.TryLock(key, Callback, timeout);

        // Assert
        isLockAcquired.Should().BeFalse();
        isCallbackInvoked.Should().BeFalse();
        collection.IsInUse(key).Should().BeTrue();

        isLockAcquired = collection.TryLock(key, Callback, Convert.ToInt32(timeout.TotalMilliseconds));

        // Assert
        isLockAcquired.Should().BeFalse();
        isCallbackInvoked.Should().BeFalse();
        collection.IsInUse(key).Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TryLock_WhenNotTimedOut_ShouldInvokeCallbackAndReturnTrue(bool useShortTimeout)
    {
        // Arrange
        var collection = new StripedAsyncKeyedLocker<string>();
        var key = "test";
        var isCallbackInvoked = false;
        var timeout = useShortTimeout
            ? _defaultSynchronousWaitDuration.Subtract(TimeSpan.FromMilliseconds(1))
            : _defaultSynchronousWaitDuration.Add(TimeSpan.FromMilliseconds(1));

        void Callback()
        {
            isCallbackInvoked = true;
        }

        // Act
        var isLockAcquired = collection.TryLock(key, Callback, timeout);

        // Assert
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();
        collection.IsInUse(key).Should().BeFalse();

        isLockAcquired = collection.TryLock(key, Callback, Convert.ToInt32(timeout.TotalMilliseconds));

        // Assert
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();
        collection.IsInUse(key).Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TryLockAsync_WhenTimedOut_ShouldNotInvokeCallbackAndReturnFalse(bool useShortTimeout)
    {
        // Arrange
        var collection = new StripedAsyncKeyedLocker<string>();
        var key = "test";
        using var _ = await collection.LockAsync(key);
        var isCallbackInvoked = false;
        var timeout = useShortTimeout
            ? _defaultSynchronousWaitDuration.Subtract(TimeSpan.FromMilliseconds(1))
            : _defaultSynchronousWaitDuration.Add(TimeSpan.FromMilliseconds(1));

        void Callback()
        {
            isCallbackInvoked = true;
        }

        // Act
        var isLockAcquired = await collection.TryLockAsync(key, Callback, timeout);

        // Assert
        isLockAcquired.Should().BeFalse();
        isCallbackInvoked.Should().BeFalse();
        collection.IsInUse(key).Should().BeTrue();

        isLockAcquired = await collection.TryLockAsync(key, Callback, Convert.ToInt32(timeout.TotalMilliseconds));

        // Assert
        isLockAcquired.Should().BeFalse();
        isCallbackInvoked.Should().BeFalse();
        collection.IsInUse(key).Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TryLockAsync_WhenNotTimedOut_ShouldNotInvokeCallbackAndReturnFalse(bool useShortTimeout)
    {
        // Arrange
        var collection = new StripedAsyncKeyedLocker<string>();
        var key = "test";
        var isCallbackInvoked = false;
        var timeout = useShortTimeout
            ? _defaultSynchronousWaitDuration.Subtract(TimeSpan.FromMilliseconds(1))
            : _defaultSynchronousWaitDuration.Add(TimeSpan.FromMilliseconds(1));

        void Callback()
        {
            isCallbackInvoked = true;
        }

        // Act
        var isLockAcquired = await collection.TryLockAsync(key, Callback, timeout);

        // Assert
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();
        collection.IsInUse(key).Should().BeFalse();

        isLockAcquired = await collection.TryLockAsync(key, Callback, Convert.ToInt32(timeout.TotalMilliseconds));

        // Assert
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();
        collection.IsInUse(key).Should().BeFalse();
    }
}