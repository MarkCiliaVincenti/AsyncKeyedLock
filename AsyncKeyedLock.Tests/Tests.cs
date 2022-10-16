using System.Collections.Concurrent;
using Xunit;

namespace AsyncKeyedLock.Tests
{
    public class Tests
    {
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
    }
}