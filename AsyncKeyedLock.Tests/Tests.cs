using System.Collections.Concurrent;
using Xunit;

namespace AsyncKeyedLock.Tests
{
    public class Tests
    {
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
                        concurrentQueue.Enqueue((true, key));
                        await Task.Delay(100);
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
                        concurrentQueue.Enqueue((true, key));
                        await Task.Delay(10);
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
            var asyncKeyedLocker = new AsyncKeyedLocker<int>(new AsyncKeyedLockOptions(poolSize: 50_000), Environment.ProcessorCount, 50_000);
            var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

            var tasks = Enumerable.Range(1, locks * concurrency)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                    using (await asyncKeyedLocker.LockAsync(key))
                    {
                        concurrentQueue.Enqueue((true, key));
                        await Task.Delay(10);
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
            var asyncKeyedLocker = new AsyncKeyedLocker<int>(new AsyncKeyedLockOptions(poolSize: Environment.ProcessorCount), Environment.ProcessorCount, 50_000);
            var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

            var tasks = Enumerable.Range(1, locks * concurrency)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                    using (await asyncKeyedLocker.LockAsync(key))
                    {
                        concurrentQueue.Enqueue((true, key));
                        await Task.Delay(10);
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
            var asyncKeyedLocker = new AsyncKeyedLocker<int>(new AsyncKeyedLockOptions(poolSize: 10_000), Environment.ProcessorCount, 50_000);
            var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

            var tasks = Enumerable.Range(1, locks * concurrency)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / concurrency));
                    using (await asyncKeyedLocker.LockAsync(key))
                    {
                        concurrentQueue.Enqueue((true, key));
                        await Task.Delay(10);
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
                        concurrentQueue.Enqueue((true, key));
                        await Task.Delay(100);
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
            var asyncKeyedLocker = new AsyncKeyedLocker(new AsyncKeyedLockOptions(2));
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
            //var asyncKeyedLocker = new AsyncKeyedLocker<int>(new AsyncKeyedLockOptions(2));
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