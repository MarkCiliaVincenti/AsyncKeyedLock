using AsyncKeyedLock.Tests.Helpers;
using FluentAssertions;
using ListShuffle;
using System.Collections;
using System.Collections.Concurrent;
using Xunit;

namespace AsyncKeyedLock.Tests.StripedAsyncKeyedLocker
{
    public class OriginalTests
    {
        [Fact]
        public void TestRecursion()
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

            double Factorial(int number, bool isFirst = true)
            {
                using (asyncKeyedLocker.ConditionalLock("test123", isFirst))
                {
                    if (number == 0)
                        return 1;
                    return number * Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, Factorial(5));
        }

        [Fact]
        public async Task TestRecursionAsync()
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

            async Task<double> Factorial(int number, bool isFirst = true)
            {
                using (await asyncKeyedLocker.ConditionalLockAsync("test123", isFirst))
                {
                    if (number == 0)
                        return 1;
                    return number * await Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, await Factorial(5));
        }

        [Fact]
        public void TestRecursionWithCancellationToken()
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

            double Factorial(int number, bool isFirst = true)
            {
                using (asyncKeyedLocker.ConditionalLock("test123", isFirst, new CancellationToken(false)))
                {
                    if (number == 0)
                        return 1;
                    return number * Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, Factorial(5));
        }

        [Fact]
        public async Task TestRecursionWithCancellationTokenAsync()
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

            async Task<double> Factorial(int number, bool isFirst = true)
            {
                using (await asyncKeyedLocker.ConditionalLockAsync("test123", isFirst, new CancellationToken(false)))
                {
                    if (number == 0)
                        return 1;
                    return number * await Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, await Factorial(5));
        }

        [Fact]
        public void TestRecursionWithTimeout()
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

            double Factorial(int number, bool isFirst = true)
            {
                using (asyncKeyedLocker.ConditionalLock("test123", isFirst, Timeout.Infinite, out _))
                {
                    if (number == 0)
                        return 1;
                    return number * Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, Factorial(5));
        }

        [Fact]
        public async Task TestRecursionWithTimeoutAsync()
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

            async Task<double> Factorial(int number, bool isFirst = true)
            {
                using (await asyncKeyedLocker.ConditionalLockAsync("test123", isFirst, Timeout.Infinite))
                {
                    if (number == 0)
                        return 1;
                    return number * await Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, await Factorial(5));
        }

        [Fact]
        public void TestRecursionWithTimeSpan()
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

            double Factorial(int number, bool isFirst = true)
            {
                using (asyncKeyedLocker.ConditionalLock("test123", isFirst, TimeSpan.Zero, out _))
                {
                    if (number == 0)
                        return 1;
                    return number * Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, Factorial(5));
        }

        [Fact]
        public async Task TestRecursionWithTimeSpanAsync()
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

            async Task<double> Factorial(int number, bool isFirst = true)
            {
                using (await asyncKeyedLocker.ConditionalLockAsync("test123", isFirst, TimeSpan.Zero))
                {
                    if (number == 0)
                        return 1;
                    return number * await Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, await Factorial(5));
        }

        [Fact]
        public void TestRecursionWithTimeoutAndCancellationToken()
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

            double Factorial(int number, bool isFirst = true)
            {
                using (asyncKeyedLocker.ConditionalLock("test123", isFirst, Timeout.Infinite, new CancellationToken(false), out _))
                {
                    if (number == 0)
                        return 1;
                    return number * Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, Factorial(5));
        }

        [Fact]
        public async Task TestRecursionWithTimeoutAndCancellationTokenAsync()
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

            async Task<double> Factorial(int number, bool isFirst = true)
            {
                using (await asyncKeyedLocker.ConditionalLockAsync("test123", isFirst, Timeout.Infinite, new CancellationToken(false)))
                {
                    if (number == 0)
                        return 1;
                    return number * await Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, await Factorial(5));
        }

        [Fact]
        public void TestRecursionWithTimeSpanAndCancellationToken()
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

            double Factorial(int number, bool isFirst = true)
            {
                using (asyncKeyedLocker.ConditionalLock("test123", isFirst, TimeSpan.Zero, new CancellationToken(false), out _))
                {
                    if (number == 0)
                        return 1;
                    return number * Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, Factorial(5));
        }

        [Fact]
        public async Task TestRecursionWithTimeSpanAndCancellationTokenAsync()
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

            async Task<double> Factorial(int number, bool isFirst = true)
            {
                using (await asyncKeyedLocker.ConditionalLockAsync("test123", isFirst, TimeSpan.Zero, new CancellationToken(false)))
                {
                    if (number == 0)
                        return 1;
                    return number * await Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, await Factorial(5));
        }

        [Fact]
        public void TestHashHelpersIsPrime0DoesNotThrow()
        {
            Action action = () =>
            {
                var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>(0);
            };
            action.Should().NotThrow();
        }

        [Fact]
        public void TestHashHelpersIsPrime1DoesNotThrow()
        {
            Action action = () =>
            {
                var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>(1);
            };
            action.Should().NotThrow();
        }

        [Fact]
        public void TestHashHelpersIsPrime7199374DoesNotThrow()
        {
            Action action = () =>
            {
                var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>(7199374);
            };
            action.Should().NotThrow();
        }

        [Fact]
        public void TestHashHelpersGetPrimeIntMaxValue()
        {
            HashHelpers.GetPrime(int.MaxValue).Should().Be(int.MaxValue);
        }

        [Fact]
        public void TestHashHelpersIsPrimeNegative1ThrowsArgumentException()
        {
            Action action = () =>
            {
                var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>(-1);
            };
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TestHashHelpersIsPrime2()
        {
            HashHelpers.IsPrime(2).Should().Be(true);
        }

        [Fact]
        public void TestComparerShouldBePossible()
        {
            Action action = () =>
            {
                var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(comparer: EqualityComparer<string>.Default);
            };
            action.Should().NotThrow();
        }

        [Fact]
        public void TestComparerAndMaxCount1ShouldBePossible()
        {
            Action action = () =>
            {
                var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(maxCount: 1, comparer: EqualityComparer<string>.Default);
            };
            action.Should().NotThrow();
        }

        [Fact]
        public void TestComparerAndMaxCount0ShouldNotBePossible()
        {
            Action action = () =>
            {
                var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(maxCount: 0, comparer: EqualityComparer<string>.Default);
            };
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void TestNumberOfStripesShouldBePossible()
        {
            Action action = () =>
            {
                var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(Environment.ProcessorCount);
            };
            action.Should().NotThrow();
        }

        [Fact]
        public void TestMaxCount0WithNumberOfStripesShouldNotBePossible()
        {
            Action action = () =>
            {
                var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(numberOfStripes: 42, maxCount: 0);
            };
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void TestReadingMaxCount()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(maxCount: 2);
            stripedAsyncKeyedLocker.MaxCount.Should().Be(2);
        }

        [Fact]
        public void TestReadingMaxCountViaParameterWithComparer()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(maxCount: 2, comparer: EqualityComparer<string>.Default);
            stripedAsyncKeyedLocker.MaxCount.Should().Be(2);
        }

        [Fact]
        public void TestReadingMaxCountViaParameterWithNumberOfStripes()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(42, 2);
            stripedAsyncKeyedLocker.MaxCount.Should().Be(2);
        }

        [Fact]
        public void TestReadingMaxCountViaParameterWithNumberOfStripesAndComparer()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(42, 2, EqualityComparer<string>.Default);
            stripedAsyncKeyedLocker.MaxCount.Should().Be(2);
        }

        [Fact]
        public async Task TestTimeoutBasic()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            using (var myLock = await stripedAsyncKeyedLocker.LockAsync("test", 0))
            {
                Assert.True(myLock.EnteredSemaphore);
                Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
            }
            Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public void TestTimeoutBasicWithOutParameter()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            using (var myLock = stripedAsyncKeyedLocker.Lock("test", 0, out var entered))
            {
                Assert.True(entered);
                Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
                stripedAsyncKeyedLocker.Lock("test", 0, out entered);
                Assert.False(entered);
                stripedAsyncKeyedLocker.Lock("test", TimeSpan.Zero, out entered);
                Assert.False(entered);
            }
            Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public async Task TestTimeout()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            using (await stripedAsyncKeyedLocker.LockAsync("test"))
            {
                using (var myLock = await stripedAsyncKeyedLocker.LockAsync("test", 0))
                {
                    Assert.False(myLock.EnteredSemaphore);
                }
                Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
            }
            Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public void TestTimeoutWithTimeSpanSynchronous()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            using (stripedAsyncKeyedLocker.Lock("test"))
            {
                using (stripedAsyncKeyedLocker.Lock("test", TimeSpan.Zero, out bool entered))
                {
                    Assert.False(entered);
                }
                Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
            }
            Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public void TestTimeoutWithInfiniteTimeoutSynchronous()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            using (stripedAsyncKeyedLocker.Lock("test", Timeout.Infinite, out bool entered))
            {
                Assert.True(entered);
                Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
            }
            Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public void TestTimeoutWithInfiniteTimeSpanSynchronous()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            using (stripedAsyncKeyedLocker.Lock("test", TimeSpan.FromMilliseconds(Timeout.Infinite), out bool entered))
            {
                Assert.True(entered);
                Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
            }
            Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public async Task TestTimeoutWithTimeSpan()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            using (await stripedAsyncKeyedLocker.LockAsync("test"))
            {
                using (var myLock = await stripedAsyncKeyedLocker.LockAsync("test", TimeSpan.Zero))
                {
                    Assert.False(myLock.EnteredSemaphore);
                }
                Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
            }
            Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public void TestTimeoutWithInfiniteTimeoutAndCancellationToken()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            using (stripedAsyncKeyedLocker.Lock("test", Timeout.Infinite, new CancellationToken(false), out bool entered))
            {
                Assert.True(entered);
                Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
            }
            Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public void TestTimeoutWithZeroTimeoutAndCancellationToken()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            using (stripedAsyncKeyedLocker.Lock("test", 0, new CancellationToken(false), out bool entered))
            {
                Assert.True(entered);
                Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
                stripedAsyncKeyedLocker.Lock("test", 0, new CancellationToken(false), out entered);
                Assert.False(entered);
                stripedAsyncKeyedLocker.Lock("test", TimeSpan.Zero, new CancellationToken(false), out entered);
                Assert.False(entered);
            }
            Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public void TestTimeoutWithZeroTimeoutAndCancelledToken()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            Action action = () =>
            {
                stripedAsyncKeyedLocker.Lock("test", 0, new CancellationToken(true), out bool entered);
            };
            action.Should().Throw<OperationCanceledException>();
            stripedAsyncKeyedLocker.IsInUse("test").Should().BeFalse();
        }

        [Fact]
        public void TestTimeoutWithInfiniteTimeSpanAndCancellationToken()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            using (stripedAsyncKeyedLocker.Lock("test", TimeSpan.FromMilliseconds(Timeout.Infinite), new CancellationToken(false), out bool entered))
            {
                Assert.True(entered);
                Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
            }
            Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public void TestTimeoutWithZeroTimeSpanAndCancellationToken()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            using (stripedAsyncKeyedLocker.Lock("test", TimeSpan.FromMilliseconds(0), new CancellationToken(false), out bool entered))
            {
                Assert.True(entered);
                Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
            }
            Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public void TestTimeoutWithZeroTimeSpanAndCancelledToken()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            Action action = () =>
            {
                stripedAsyncKeyedLocker.Lock("test", TimeSpan.FromMilliseconds(0), new CancellationToken(true), out bool entered);
            };
            action.Should().Throw<OperationCanceledException>();
            stripedAsyncKeyedLocker.IsInUse("test").Should().BeFalse();
        }

        [Fact]
        public void TestTimeoutTryLock()
        {
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            using (stripedAsyncKeyedLocker.Lock("test", TimeSpan.Zero, out bool entered))
            {
                Assert.True(entered);
                Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
                Assert.False(stripedAsyncKeyedLocker.TryLock("test", () => { }, 0, CancellationToken.None));
                Assert.False(stripedAsyncKeyedLocker.TryLock("test", () => { }, TimeSpan.Zero, CancellationToken.None));
            }
            Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
        }

        [Fact]
        public async Task BasicTest()
        {
            var locks = 5000;
            var concurrency = 50;
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<object>();
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
        public async Task BasicTestGenerics()
        {
            var locks = 5000;
            var concurrency = 50;
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<int>();
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
            StripedAsyncKeyedLocker<string> AsyncKeyedLocker;
            ParallelQuery<Task>? AsyncKeyedLockerTasks = null;
            Dictionary<int, List<int>> _shuffledIntegers = new();

            var NumberOfLocks = 200;
            var Contention = 100;
            var GuidReversals = 0;

            if (!_shuffledIntegers.TryGetValue(Contention * NumberOfLocks, out var ShuffledIntegers))
            {
                ShuffledIntegers = Enumerable.Range(0, Contention * NumberOfLocks).ToList();
                ShuffledIntegers.Shuffle();
                _shuffledIntegers[Contention * NumberOfLocks] = ShuffledIntegers;
            }

            if (NumberOfLocks != Contention)
            {
                AsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(NumberOfLocks);
                AsyncKeyedLockerTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await AsyncKeyedLocker.LockAsync(key.ToString()))
                        {
                            for (int j = 0; j < GuidReversals; j++)
                            {
                                Guid guid = Guid.NewGuid();
                                var guidString = guid.ToString();
                                guidString = guidString.Reverse().ToString();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                                if (guidString.Length != 53)
                                {
                                    throw new Exception($"Not 53 but {guidString?.Length}");
                                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                            }
                        }

                        await Task.Yield();
                    }).AsParallel();

                await Task.WhenAll(AsyncKeyedLockerTasks);
            }
        }

        [Fact]
        public async Task BasicTestGenericsString()
        {
            var locks = 5000;
            var concurrency = 50;
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            var concurrentQueue = new ConcurrentQueue<(bool entered, string key)>();

            var tasks = Enumerable.Range(1, locks * concurrency)
                .Select(async i =>
                {
                    var key = Convert.ToInt32(Math.Ceiling((double)i / 5)).ToString();
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
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<object>();
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
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<object>(maxCount: 2);
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
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<int>();
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
            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<int>(maxCount: 2);
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
        public Task TestContinueOnCapturedContextTrue()
            => TestContinueOnCapturedContext(true);

        [Fact]
        public Task TestContinueOnCapturedContextFalse()
            => TestContinueOnCapturedContext(false);

        private async Task TestContinueOnCapturedContext(bool continueOnCapturedContext)
        {
            const string Key = "test";

            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            var testContext = new TestSynchronizationContext();

            void Callback()
            {
                if (continueOnCapturedContext)
                {
                    Environment.CurrentManagedThreadId.Should().Be(testContext.LastPostThreadId);
                }
                else
                {
                    testContext.LastPostThreadId.Should().Be(default);
                }
            }

            var previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(testContext);

            try
            {
                // This is just to make WaitAsync in TryLockAsync not finish synchronously
                var obj = stripedAsyncKeyedLocker.Lock(Key);

                _ = Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    obj.Dispose();
                });

                await stripedAsyncKeyedLocker.TryLockAsync(Key, Callback, 5000, continueOnCapturedContext);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        [Fact]
        public Task TestOptionContinueOnCapturedContext()
                    => TestConfigureAwaitOptions(ConfigureAwaitOptions.ContinueOnCapturedContext);

        [Fact]
        public Task TestOptionForceYielding()
            => TestConfigureAwaitOptions(ConfigureAwaitOptions.ForceYielding);

        private async Task TestConfigureAwaitOptions(ConfigureAwaitOptions configureAwaitOptions)
        {
            const string Key = "test";

            var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            var testContext = new TestSynchronizationContext();

            void Callback()
            {
                if (configureAwaitOptions == ConfigureAwaitOptions.ContinueOnCapturedContext)
                {
                    Environment.CurrentManagedThreadId.Should().Be(testContext.LastPostThreadId);
                }
                else
                {
                    testContext.LastPostThreadId.Should().Be(default);
                }
            }

            var previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(testContext);

            try
            {
                // This is just to make WaitAsync in TryLockAsync not finish synchronously
                var obj = stripedAsyncKeyedLocker.Lock(Key);

                _ = Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    obj.Dispose();
                });

                await stripedAsyncKeyedLocker.TryLockAsync(Key, Callback, 5000, configureAwaitOptions);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }
    }
}