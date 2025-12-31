using System.Collections.Concurrent;
using System.Globalization;
using FluentAssertions;
using ListShuffle;
using Xunit;

namespace AsyncKeyedLock.Tests.StripedAsyncKeyedLocker;

[Collection("Stress Tests")]
[CollectionDefinition("Stress Tests", DisableParallelization = true)]
public class StressTests
{
    [Fact]
    public async Task ConcurrentDispose()
    {
        Func<Task> action = async () =>
        {
            var concurrency = 50;
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<object>();
            var tasks = Enumerable.Range(1, concurrency)
                .Select(i => Task.Run(() => asyncKeyedLocker.Dispose()));
            await Task.WhenAll(tasks);
        };

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StressTest()
    {
        var locks = 5000;
        var concurrency = 50;
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<object>();
        var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

        var tasks = Enumerable.Range(1, locks * concurrency)
            .Select(async i =>
            {
                var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                using (await stripedAsyncKeyedLocker.LockAsync(key))
                {
                    await Task.Delay(20);
                    concurrentQueue.Enqueue((true, key));
                    await Task.Delay(80);
                    concurrentQueue.Enqueue((false, key));
                }
            });
        await Task.WhenAll(tasks.AsParallel());

        bool valid = concurrentQueue.Count == locks * concurrency * 2;

        var entered = new HashSet<int>();

        while (valid && !concurrentQueue.IsEmpty)
        {
            concurrentQueue.TryDequeue(out var result);
            if (result.entered)
            {
                if (entered.Contains(result.key))
                {
                    valid = false;
                    break;
                }
                entered.Add(result.key);
            }
            else
            {
                if (!entered.Contains(result.key))
                {
                    valid = false;
                    break;
                }
                entered.Remove(result.key);
            }
        }

        Assert.True(valid);
    }

    [Fact]
    public async Task StressTestGenerics()
    {
        var locks = 5000;
        var concurrency = 50;
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<int>();
        var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

        var tasks = Enumerable.Range(1, locks * concurrency)
            .Select(async i =>
            {
                var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                using (await stripedAsyncKeyedLocker.LockAsync(key))
                {
                    await Task.Delay(20);
                    concurrentQueue.Enqueue((true, key));
                    await Task.Delay(80);
                    concurrentQueue.Enqueue((false, key));
                }
            });
        await Task.WhenAll(tasks.AsParallel());

        bool valid = concurrentQueue.Count == locks * concurrency * 2;

        var entered = new HashSet<int>();

        while (valid && !concurrentQueue.IsEmpty)
        {
            concurrentQueue.TryDequeue(out var result);
            if (result.entered)
            {
                if (entered.Contains(result.key))
                {
                    valid = false;
                    break;
                }
                entered.Add(result.key);
            }
            else
            {
                if (!entered.Contains(result.key))
                {
                    valid = false;
                    break;
                }
                entered.Remove(result.key);
            }
        }

        Assert.True(valid);
    }

    [Fact]
    public async Task BenchmarkSimulationTest()
    {
        StripedAsyncKeyedLocker<string> asyncKeyedLocker;
        ParallelQuery<Task>? asyncKeyedLockerTasks = null;
        Dictionary<int, List<int>> shuffledIntegers = [];

        var numberOfLocks = 200;
        var contention = 100;
        var guidReversals = 0;

        if (!shuffledIntegers.TryGetValue(contention * numberOfLocks, out var ints))
        {
            ints = [.. Enumerable.Range(0, contention * numberOfLocks)];
            ints.Shuffle();
            shuffledIntegers[contention * numberOfLocks] = ints;
        }

        asyncKeyedLocker = new StripedAsyncKeyedLocker<string>(numberOfLocks);
        asyncKeyedLockerTasks = ints
            .Select(async i =>
            {
                var key = i % numberOfLocks;

                using (var myLock = await asyncKeyedLocker.LockAsync(key.ToString(CultureInfo.InvariantCulture)))
                {
                    for (int j = 0; j < guidReversals; j++)
                    {
                        Guid guid = Guid.NewGuid();
                        var guidString = guid.ToString();
                        guidString = guidString.Reverse().ToString();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        if (guidString.Length != 53)
                        {
                            throw new InvalidOperationException($"Not 53 but {guidString?.Length}");
                        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    }
                }

                await Task.Yield();
            }).AsParallel();

        await Task.WhenAll(asyncKeyedLockerTasks);

        asyncKeyedLocker.Dispose();
    }

    [Fact]
    public async Task StressTestGenericsString()
    {
        var locks = 5000;
        var concurrency = 50;
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        var concurrentQueue = new ConcurrentQueue<(bool entered, string key)>();

        var tasks = Enumerable.Range(1, locks * concurrency)
            .Select(async i =>
            {
                var key = Convert.ToInt32(Math.Ceiling((double)i / 5)).ToString(CultureInfo.InvariantCulture);
                using (await stripedAsyncKeyedLocker.LockAsync(key))
                {
                    await Task.Delay(20);
                    concurrentQueue.Enqueue((true, key));
                    await Task.Delay(80);
                    concurrentQueue.Enqueue((false, key));
                }
            });
        await Task.WhenAll(tasks.AsParallel());

        bool valid = concurrentQueue.Count == locks * concurrency * 2;

        var entered = new HashSet<string>();

        while (valid && !concurrentQueue.IsEmpty)
        {
            concurrentQueue.TryDequeue(out var result);
            if (result.entered)
            {
                if (entered.Contains(result.key))
                {
                    valid = false;
                    break;
                }
                entered.Add(result.key);
            }
            else
            {
                if (!entered.Contains(result.key))
                {
                    valid = false;
                    break;
                }
                entered.Remove(result.key);
            }
        }

        Assert.True(valid);
    }

    [Fact]
    public async Task Test1AtATime()
    {
        var range = 25000;
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<object>();
        var concurrentQueue = new ConcurrentQueue<int>();

        int threadNum = 0;

        var tasks = Enumerable.Range(1, range * 2)
            .Select(async i =>
            {
                var key = Convert.ToInt32(Math.Ceiling((double)Interlocked.Increment(ref threadNum) / 2));
                using (await stripedAsyncKeyedLocker.LockAsync(key))
                {
                    concurrentQueue.Enqueue(key);
                }
            });
        await Task.WhenAll(tasks.AsParallel());

        bool valid = true;
        var list = concurrentQueue.ToList();

        for (int i = 0; i < range; i++)
        {
            if (list[i] == list[i + range])
            {
                valid = false;
                break;
            }
        }

        Assert.True(valid);
    }

    [Fact]
    public async Task Test2AtATime()
    {
        var range = 4;
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<object>(maxCount: 2);
        var concurrentQueue = new ConcurrentQueue<int>();

        var tasks = Enumerable.Range(1, range * 4)
            .Select(async i =>
            {
                var key = Convert.ToInt32(Math.Ceiling((double)i / 4));
                using (await stripedAsyncKeyedLocker.LockAsync(key))
                {
                    concurrentQueue.Enqueue(key);
                    await Task.Delay((100 * key) + 1000);
                }
            }).ToList();
        await Task.WhenAll(tasks);

        bool valid = true;
        var list = concurrentQueue.ToList();

        for (int i = 0; i < range * 2; i++)
        {
            if (list[i] != list[i + (range * 2)])
            {
                valid = false;
                break;
            }
        }

        Assert.True(valid);
    }

    [Fact]
    public async Task Test1AtATimeGenerics()
    {
        var range = 25000;
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<int>();
        var concurrentQueue = new ConcurrentQueue<int>();

        int threadNum = 0;

        var tasks = Enumerable.Range(1, range * 2)
            .Select(async i =>
            {
                var key = Convert.ToInt32(Math.Ceiling((double)Interlocked.Increment(ref threadNum) / 2));
                using (await stripedAsyncKeyedLocker.LockAsync(key))
                {
                    concurrentQueue.Enqueue(key);
                }
            });
        await Task.WhenAll(tasks.AsParallel());

        bool valid = true;
        var list = concurrentQueue.ToList();

        for (int i = 0; i < range; i++)
        {
            if (list[i] == list[i + range])
            {
                valid = false;
                break;
            }
        }

        Assert.True(valid);
    }

    [Fact]
    public async Task Test2AtATimeGenerics()
    {
        var range = 4;
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<int>(maxCount: 2);
        var concurrentQueue = new ConcurrentQueue<int>();

        var tasks = Enumerable.Range(1, range * 4)
            .Select(async i =>
            {
                var key = Convert.ToInt32(Math.Ceiling((double)i / 4));
                using (await stripedAsyncKeyedLocker.LockAsync(key))
                {
                    concurrentQueue.Enqueue(key);
                    await Task.Delay((100 * key) + 1000);
                }
            }).ToList();
        await Task.WhenAll(tasks);

        bool valid = true;
        var list = concurrentQueue.ToList();

        for (int i = 0; i < range * 2; i++)
        {
            if (list[i] != list[i + (range * 2)])
            {
                valid = false;
                break;
            }
        }

        Assert.True(valid);
    }
}
