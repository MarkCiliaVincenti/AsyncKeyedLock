using FluentAssertions;
using System.Collections.Concurrent;
using Xunit;

namespace AsyncKeyedLock.Tests.AsyncKeyedLocker;

/// <summary>
/// Adapted from https://github.com/amoerie/keyed-semaphores/blob/main/KeyedSemaphores.Tests/TestsForKeyedSemaphore.cs
/// </summary>
public class TestsForAsyncKeyedLock
{
    private static readonly AsyncKeyedLocker<string> _asyncKeyedLocker = new AsyncKeyedLocker<string>();

    public class Async : TestsForAsyncKeyedLock
    {
        [Fact]
        public async Task ShouldRunThreadsWithDistinctKeysInParallel()
        {
            // Arrange
            var currentParallelism = 0;
            var maxParallelism = 0;
            var parallelismLock = new object();

            // 100 threads, 100 keys
            var threads = Enumerable.Range(0, 100)
                .Select(i => Task.Run(async () => await OccupyTheLockALittleBit(i).ConfigureAwait(false)))
                .ToList();

            // Act
            await Task.WhenAll(threads).ConfigureAwait(false);

            maxParallelism.Should().BeGreaterThan(10);

            async Task OccupyTheLockALittleBit(int key)
            {
                using (await _asyncKeyedLocker.LockAsync(key.ToString()))
                {
                    var incrementedCurrentParallelism = Interlocked.Increment(ref currentParallelism);

                    lock (parallelismLock)
                    {
                        maxParallelism = Math.Max(incrementedCurrentParallelism, maxParallelism);
                    }

                    const int delay = 250;

                    await Task.Delay(TimeSpan.FromMilliseconds(delay)).ConfigureAwait(false);

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

            // 100 threads, 10 keys
            var threads = Enumerable.Range(0, 100)
                .Select(i => Task.Run(async () => await OccupyTheLockALittleBit(i % 10).ConfigureAwait(false)))
                .ToList();

            // Act + Assert
            await Task.WhenAll(threads).ConfigureAwait(false);

            maxParallelism.Should().BeLessOrEqualTo(10);

            async Task OccupyTheLockALittleBit(int key)
            {
                using (await _asyncKeyedLocker.LockAsync(key.ToString()))
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

                    await Task.Delay(TimeSpan.FromMilliseconds(delay)).ConfigureAwait(false);

                    if (!runningTasksIndex.TryRemove(key, out var value))
                    {
                        var ex = new Exception($"Thread #{currentTaskId} has finished " +
                                               "but when trying to cleanup the running threads index, the value is already gone");

                        throw ex;
                    }

                    if (value != currentTaskId)
                    {
                        var ex = new Exception($"Thread #{currentTaskId} has finished and has removed itself from the running threads index," +
                                               $" but that index contained an incorrect value: #{value}!");

                        throw ex;
                    }

                    Interlocked.Decrement(ref currentParallelism);
                }
            }
        }
    }

    public class Sync : TestsForAsyncKeyedLock
    {
        [Fact]
        public void ShouldRunThreadsWithDistinctKeysInParallel()
        {
            // Arrange
            var currentParallelism = 0;
            var maxParallelism = 0;
            var parallelismLock = new object();

            // 100 threads, 100 keys
            var threads = Enumerable.Range(0, 100)
                .Select(i => new Thread(() => OccupyTheLockALittleBit(i)))
                .ToList();

            // Act
            foreach (var thread in threads) thread.Start();

            foreach (var thread in threads) thread.Join();

            maxParallelism.Should().BeGreaterThan(10);

            void OccupyTheLockALittleBit(int key)
            {
                using (_asyncKeyedLocker.Lock(key.ToString()))
                {
                    var incrementedCurrentParallelism = Interlocked.Increment(ref currentParallelism);

                    lock (parallelismLock)
                    {
                        maxParallelism = Math.Max(incrementedCurrentParallelism, maxParallelism);
                    }

                    const int delay = 250;

                    Thread.Sleep(TimeSpan.FromMilliseconds(delay));

                    Interlocked.Decrement(ref currentParallelism);
                }
            }
        }

        [Fact]
        public void ShouldRunThreadsWithSameKeysLinearly()
        {
            // Arrange
            var runningThreadsIndex = new ConcurrentDictionary<int, int>();
            var parallelismLock = new object();
            var currentParallelism = 0;
            var maxParallelism = 0;

            // 100 threads, 10 keys
            var threads = Enumerable.Range(0, 100)
                .Select(i => new Thread(() => OccupyTheLockALittleBit(i % 10)))
                .ToList();

            // Act
            foreach (var thread in threads) thread.Start();

            foreach (var thread in threads) thread.Join();

            // Assert
            maxParallelism.Should().BeLessOrEqualTo(10);

            void OccupyTheLockALittleBit(int key)
            {
                using (_asyncKeyedLocker.Lock(key.ToString()))
                {
                    var incrementedCurrentParallelism = Interlocked.Increment(ref currentParallelism);

                    lock (parallelismLock)
                    {
                        maxParallelism = Math.Max(incrementedCurrentParallelism, maxParallelism);
                    }

                    var currentThreadId = Thread.CurrentThread.ManagedThreadId;
                    if (runningThreadsIndex.TryGetValue(key, out var otherThread))
                        throw new Exception($"Thread #{currentThreadId} acquired a lock using key ${key} " +
                                            $"but another thread #{otherThread} is also still running using this key!");

                    runningThreadsIndex[key] = currentThreadId;

                    const int delay = 10;

                    Thread.Sleep(TimeSpan.FromMilliseconds(delay));

                    if (!runningThreadsIndex.TryRemove(key, out var value))
                    {
                        var ex = new Exception($"Thread #{currentThreadId} has finished " +
                                               "but when trying to cleanup the running threads index, the value is already gone");

                        throw ex;
                    }

                    if (value != currentThreadId)
                    {
                        var ex = new Exception($"Thread #{currentThreadId} has finished and has removed itself from the running threads index," +
                                               $" but that index contained an incorrect value: #{value}!");

                        throw ex;
                    }

                    Interlocked.Decrement(ref currentParallelism);
                }
            }
        }
    }
}
