using System.Collections.Concurrent;
using Xunit;

namespace AsyncKeyedLock.Tests
{
    public class Tests
    {
        [Fact]
        public async Task BasicTest()
        {
            var range = 5000;
            var asyncKeyedLocker = new AsyncKeyedLocker();
            var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

            var tasks = Enumerable.Range(1, range * 5)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / 5));
                    using (await asyncKeyedLocker.LockAsync(key))
                    {
                        concurrentQueue.Enqueue((true, key));
                        await Task.Delay(100);
                        concurrentQueue.Enqueue((false, key));
                    }
                });
            await Task.WhenAll(tasks.AsParallel());

            bool valid = true;

            var entered = new HashSet<int>();

            while (!concurrentQueue.IsEmpty)
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
        }

        [Fact]
        public async Task BasicTestGenerics()
        {
            var range = 5000;
            var asyncKeyedLocker = new AsyncKeyedLocker<int>();
            var concurrentQueue = new ConcurrentQueue<(bool entered, int key)>();

            var tasks = Enumerable.Range(1, range * 5)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / 5));
                    using (await asyncKeyedLocker.LockAsync(key))
                    {
                        concurrentQueue.Enqueue((true, key));
                        await Task.Delay(100);
                        concurrentQueue.Enqueue((false, key));
                    }
                });
            await Task.WhenAll(tasks.AsParallel());

            bool valid = true;

            var entered = new HashSet<int>();

            while (!concurrentQueue.IsEmpty)
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
            var asyncKeyedLocker = new AsyncKeyedLocker(2);
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
            var asyncKeyedLocker = new AsyncKeyedLocker<int>(2);
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