using System.Collections.Concurrent;
using Xunit;

namespace AsyncKeyedLock.Tests
{
    public class OriginalTests
    {
        [Fact]
        public async Task TestTimeout()
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>();
            using (var myFirstLock = await asyncKeyedLocker.LockAsync("test"))
            {
                using (var myLock = await asyncKeyedLocker.LockAsync("test", 0))
                {
                    var myTimeoutReleaser = (AsyncKeyedLockTimeoutReleaser<string>)myLock;
                    Assert.False(myTimeoutReleaser.EnteredSemaphore);
                }
                Assert.True(asyncKeyedLocker.IsInUse("test"));
            }
            Assert.False(asyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public void TestTimeoutWithTimeSpanSynchronous()
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>();
            using (var myFirstLock = asyncKeyedLocker.Lock("test"))
            {
                using (var myLock = asyncKeyedLocker.Lock("test", TimeSpan.Zero, out bool entered))
                {
                    Assert.False(entered);
                }
                Assert.True(asyncKeyedLocker.IsInUse("test"));
            }
            Assert.False(asyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public async Task TestTimeoutWithTimeSpan()
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>();
            using (var myFirstLock = await asyncKeyedLocker.LockAsync("test"))
            {
                using (var myLock = await asyncKeyedLocker.LockAsync("test", TimeSpan.Zero))
                {
                    var myTimeoutReleaser = (AsyncKeyedLockTimeoutReleaser<string>)myLock;
                    Assert.False(myTimeoutReleaser.EnteredSemaphore);
                }
                Assert.True(asyncKeyedLocker.IsInUse("test"));
            }
            Assert.False(asyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public async Task BasicTest()
        {
            var locks = 5000;
            var concurrency = 50;
            var asyncKeyedLocker = new AsyncKeyedLocker();
            var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

            var tasks = Enumerable.Range(1, locks * concurrency)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                    using (await asyncKeyedLocker.LockAsync(key))
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
        public async Task BasicTestGenerics()
        {
            var locks = 50_000;
            var concurrency = 50;
            var asyncKeyedLocker = new AsyncKeyedLocker<int>();
            var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

            var tasks = Enumerable.Range(1, locks * concurrency)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                    using (await asyncKeyedLocker.LockAsync(key))
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
        public async Task BasicTestGenericsPooling50k()
        {
            var locks = 50_000;
            var concurrency = 50;
            var asyncKeyedLocker = new AsyncKeyedLocker<int>(o => o.PoolSize = 50_000, Environment.ProcessorCount, 50_000);
            var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

            var tasks = Enumerable.Range(1, locks * concurrency)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                    using (await asyncKeyedLocker.LockAsync(key))
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
        public async Task BasicTestGenericsPooling50kUnfilled()
        {
            var locks = 50_000;
            var concurrency = 50;
            var asyncKeyedLocker = new AsyncKeyedLocker<int>(o =>
                {
                    o.PoolSize = 50_000;
                    o.PoolInitialFill = 0;
                }, Environment.ProcessorCount, 50_000);
            var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

            var tasks = Enumerable.Range(1, locks * concurrency)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                    using (await asyncKeyedLocker.LockAsync(key))
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
        public async Task BasicTestGenericsPoolingProcessorCount()
        {
            var locks = 50_000;
            var concurrency = 50;
            var asyncKeyedLocker = new AsyncKeyedLocker<int>(o => o.PoolSize = Environment.ProcessorCount, Environment.ProcessorCount, 50_000);
            var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

            var tasks = Enumerable.Range(1, locks * concurrency)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                    using (await asyncKeyedLocker.LockAsync(key))
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
        public async Task BasicTestGenericsPooling10k()
        {
            var locks = 50_000;
            var concurrency = 50;
            var asyncKeyedLocker = new AsyncKeyedLocker<int>(o => o.PoolSize = 10_000, Environment.ProcessorCount, 50_000);
            var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

            var tasks = Enumerable.Range(1, locks * concurrency)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                    using (await asyncKeyedLocker.LockAsync(key))
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
        public async Task BasicTestGenericsString()
        {
            var locks = 5000;
            var concurrency = 50;
            var asyncKeyedLocker = new AsyncKeyedLocker<string>();
            var concurrentQueue = new ConcurrentQueue<(bool entered, string key)>();

            var tasks = Enumerable.Range(1, locks * concurrency)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / 5)).ToString();
                    using (await asyncKeyedLocker.LockAsync(key))
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
            var range = 25;
            var asyncKeyedLocker = new AsyncKeyedLocker();
            var concurrentQueue = new ConcurrentQueue<int>();

            var tasks = Enumerable.Range(1, range * 2)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / 2));
                    using (await asyncKeyedLocker.LockAsync(key))
                    {
                        concurrentQueue.Enqueue(key);
                        await Task.Delay(100 * key);
                    }
                });
            await Task.WhenAll(tasks.AsParallel());

            bool valid = true;
            var list = concurrentQueue.ToList();

            for (int i = 0; i < range; i++)
            {
                if (list[i] != list[i + range])
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
            var asyncKeyedLocker = new AsyncKeyedLocker(o => o.MaxCount = 2);
            var concurrentQueue = new ConcurrentQueue<int>();

            var tasks = Enumerable.Range(1, range * 4)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / 4));
                    using (await asyncKeyedLocker.LockAsync(key))
                    {
                        concurrentQueue.Enqueue(key);
                        await Task.Delay((100 * key) + 1000);
                    }
                });
            await Task.WhenAll(tasks.AsParallel());

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
            var range = 25;
            var asyncKeyedLocker = new AsyncKeyedLocker<int>();
            var concurrentQueue = new ConcurrentQueue<int>();

            var tasks = Enumerable.Range(1, range * 2)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / 2));
                    using (await asyncKeyedLocker.LockAsync(key))
                    {
                        concurrentQueue.Enqueue(key);
                        await Task.Delay(100 * key);
                    }
                });
            await Task.WhenAll(tasks.AsParallel());

            bool valid = true;
            var list = concurrentQueue.ToList();

            for (int i = 0; i < range; i++)
            {
                if (list[i] != list[i + range])
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
            var asyncKeyedLocker = new AsyncKeyedLocker<int>(o => o.MaxCount = 2);
            var concurrentQueue = new ConcurrentQueue<int>();

            var tasks = Enumerable.Range(1, range * 4)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / 4));
                    using (await asyncKeyedLocker.LockAsync(key))
                    {
                        concurrentQueue.Enqueue(key);
                        await Task.Delay((100 * key) + 1000);
                    }
                });
            await Task.WhenAll(tasks.AsParallel());

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
}