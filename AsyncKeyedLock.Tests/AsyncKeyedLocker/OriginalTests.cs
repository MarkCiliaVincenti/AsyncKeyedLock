using AsyncKeyedLock.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace AsyncKeyedLock.Tests.AsyncKeyedLocker;

[Collection("Original Tests")]
[CollectionDefinition("Original Tests", DisableParallelization = false)]
public class OriginalTests
{
    public void Null_Parameters_Should_Throw()
    {
        Action options = () =>
        {
            AsyncKeyedLockOptions options = null!;
            using var asyncKeyedLocker = new AsyncKeyedLocker<string>(options);
        };
        Action actionOptions = () =>
        {
            Action<AsyncKeyedLockOptions> options = null!;
            using var asyncKeyedLocker = new AsyncKeyedLocker<string>(options);
        };
        Action optionsWithComparer = () =>
        {
            AsyncKeyedLockOptions options = null!;
            using var asyncKeyedLocker = new AsyncKeyedLocker<string>(options, StringComparer.Ordinal);
        };
        Action actionOptionsWithComparer = () =>
        {
            Action<AsyncKeyedLockOptions> options = null!;
            using var asyncKeyedLocker = new AsyncKeyedLocker<string>(options, StringComparer.Ordinal);
        };
        Action optionsWithConcurrencyLevelAndCapacity = () =>
        {
            AsyncKeyedLockOptions options = null!;
            using var asyncKeyedLocker = new AsyncKeyedLocker<string>(options, 1, 1);
        };
        Action actionOptionsWithConcurrencyLevelAndCapacity = () =>
        {
            Action<AsyncKeyedLockOptions> options = null!;
            using var asyncKeyedLocker = new AsyncKeyedLocker<string>(options, 1, 1);
        };
        Action optionsWithConcurrencyLevelAndCapacityAndComparer = () =>
        {
            AsyncKeyedLockOptions options = null!;
            using var asyncKeyedLocker = new AsyncKeyedLocker<string>(options, 1, 1, StringComparer.Ordinal);
        };
        Action actionOptionsWithConcurrencyLevelAndCapacityAndComparer = () =>
        {
            Action<AsyncKeyedLockOptions> options = null!;
            using var asyncKeyedLocker = new AsyncKeyedLocker<string>(options, 1, 1, StringComparer.Ordinal);
        };

        options.Should().Throw<ArgumentNullException>();
        actionOptions.Should().Throw<ArgumentNullException>();
        optionsWithComparer.Should().Throw<ArgumentNullException>();
        actionOptionsWithComparer.Should().Throw<ArgumentNullException>();
        optionsWithConcurrencyLevelAndCapacity.Should().Throw<ArgumentNullException>();
        actionOptionsWithConcurrencyLevelAndCapacity.Should().Throw<ArgumentNullException>();
        optionsWithConcurrencyLevelAndCapacityAndComparer.Should().Throw<ArgumentNullException>();
        actionOptionsWithConcurrencyLevelAndCapacityAndComparer.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DoubleDispose_ShouldNotThrow()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
            asyncKeyedLocker.Dispose();
            asyncKeyedLocker.Dispose();
        };
        action.Should().NotThrow();            
    }

    [Fact]
    public void TestRecursion()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 1; o.PoolInitialFill = -1; });

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
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => o.PoolSize = 1);

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
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => o.PoolSize = 1);

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
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => o.PoolSize = 1);

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
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>();

        double Factorial(int number, bool isFirst = true)
        {
            using (asyncKeyedLocker.ConditionalLock("test123", isFirst, Timeout.Infinite))
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
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>();

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
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => o.PoolSize = 1);

        double Factorial(int number, bool isFirst = true)
        {
            using (asyncKeyedLocker.ConditionalLock("test123", isFirst, TimeSpan.Zero))
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
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => o.PoolSize = 1);

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
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => o.PoolSize = 1);

        double Factorial(int number, bool isFirst = true)
        {
            using (asyncKeyedLocker.ConditionalLock("test123", isFirst, Timeout.Infinite, new CancellationToken(false)))
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
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => o.PoolSize = 1);

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
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => o.PoolSize = 1);

        double Factorial(int number, bool isFirst = true)
        {
            using (asyncKeyedLocker.ConditionalLock("test123", isFirst, TimeSpan.Zero, new CancellationToken(false)))
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
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => o.PoolSize = 1);

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
    public void IsInUseRaceConditionCoverage()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => o.PoolSize = 1);
        var releaser = asyncKeyedLocker._dictionary._pool!.GetObject("test");
        asyncKeyedLocker._dictionary._pool.PutObject(releaser);
        asyncKeyedLocker.Lock("test");
        releaser.IsNotInUse = true; // internal
        Assert.False(asyncKeyedLocker.IsInUse("test"));
        asyncKeyedLocker._dictionary.PoolingEnabled = false; // internal
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void IsInUseKeyChangeRaceConditionCoverage()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => o.PoolSize = 1);
        var releaser = asyncKeyedLocker._dictionary._pool!.GetObject("test");
        asyncKeyedLocker._dictionary._pool.PutObject(releaser);
        asyncKeyedLocker.Lock("test");
        releaser.Key = "test2"; // internal
        Assert.False(asyncKeyedLocker.IsInUse("test"));
        asyncKeyedLocker._dictionary.PoolingEnabled = false; // internal
        Assert.True(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TryIncrementNoPoolingCoverage()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => o.PoolSize = 0);
        var releaser = (AsyncKeyedLockReleaser<string>)asyncKeyedLocker.Lock("test");
        releaser.IsNotInUse = true;
        using var timer = new System.Timers.Timer(1000);
        timer.Elapsed += (_, _) => { releaser.Dispose(); };
        timer.Start();
        using (asyncKeyedLocker.Lock("test"))
        {
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestMaxCountLessThan1()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.MaxCount = 0; o.PoolSize = 0; });
        };
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TestMaxCount1ShouldNotThrow()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(o =>
            {
                o.MaxCount = 1;
                o.PoolSize = 1;
            });
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestComparerShouldBePossible()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(EqualityComparer<string>.Default);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestComparerAndMaxCount1ShouldBePossible()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(o =>
            {
                o.MaxCount = 1;
                o.PoolSize = 1;
            }, EqualityComparer<string>.Default);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestComparerAndMaxCount0ShouldNotBePossible()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.MaxCount = 0; o.PoolSize = 0; }, EqualityComparer<string>.Default);
        };
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TestConcurrencyLevelAndCapacityShouldBePossible()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(Environment.ProcessorCount, 100);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestMaxCount0WithConcurrencyLevelAndCapacityShouldNotBePossible()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.MaxCount = 0; o.PoolSize = 0; }, Environment.ProcessorCount, 100);
        };
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TestConcurrencyLevelAndCapacityAndComparerShouldBePossible()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(Environment.ProcessorCount, 100, EqualityComparer<string>.Default);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestMaxCount0AndConcurrencyLevelAndCapacityAndComparerShouldNotBePossible()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.MaxCount = 0; o.PoolSize = 0; }, Environment.ProcessorCount, 100, EqualityComparer<string>.Default);
        };
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TestMaxCount1AndConcurrencyLevelAndCapacityAndComparerShouldBePossible()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(o =>
            {
                o.MaxCount = 1;
                o.PoolSize = 1;
            }, Environment.ProcessorCount, 100, EqualityComparer<string>.Default);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestDisposeDoesNotThrow()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(o =>
            {
                o.PoolSize = 20;
                o.PoolInitialFill = 1;
            });
            asyncKeyedLocker.Lock("test");
            asyncKeyedLocker.Dispose();
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestDisposeDoesNotThrowDespiteDisposedSemaphoreSlim()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new AsyncKeyedLocker<string>(o =>
            {
                o.PoolSize = 20;
                o.PoolInitialFill = 1;
            });
            AsyncKeyedLockReleaser<string> releaser = (AsyncKeyedLockReleaser<string>)asyncKeyedLocker.Lock("test");
            releaser.SemaphoreSlim.Dispose();
            asyncKeyedLocker.Dispose();
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestIndexDoesNotThrow()
    {
        var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        asyncKeyedLocker.Lock("test");
        asyncKeyedLocker.Index.Count.Should().Be(1);
        asyncKeyedLocker.Dispose();
    }

    [Fact]
    public void TestReadingMaxCount()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.MaxCount = 2; o.PoolSize = 0; });
        asyncKeyedLocker.MaxCount.Should().Be(2);
    }

    [Fact]
    public void TestReadingMaxCountViaParameter()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(new AsyncKeyedLockOptions(2, 0));
        asyncKeyedLocker.MaxCount.Should().Be(2);
    }

    [Fact]
    public void TestReadingMaxCountViaParameterWithComparer()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(new AsyncKeyedLockOptions(2, 0), EqualityComparer<string>.Default);
        asyncKeyedLocker.MaxCount.Should().Be(2);
    }

    [Fact]
    public void TestReadingMaxCountViaParameterWithConcurrencyLevelAndCapacity()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(new AsyncKeyedLockOptions(2, 0), Environment.ProcessorCount, 100);
        asyncKeyedLocker.MaxCount.Should().Be(2);
    }

    [Fact]
    public void TestReadingMaxCountViaParameterWithConcurrencyLevelAndCapacityAndComparer()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(new AsyncKeyedLockOptions(2, 0), Environment.ProcessorCount, 100, EqualityComparer<string>.Default);
        asyncKeyedLocker.MaxCount.Should().Be(2);
    }

    [Fact]
    public void TestGetCurrentCount()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        asyncKeyedLocker.GetRemainingCount("test").Should().Be(0);
        asyncKeyedLocker.GetCurrentCount("test").Should().Be(1);
        asyncKeyedLocker.Lock("test");
        asyncKeyedLocker.GetRemainingCount("test").Should().Be(1);
        asyncKeyedLocker.GetCurrentCount("test").Should().Be(0);
    }

    [Fact]
    public async Task TestTimeoutBasic()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (var myLock = await asyncKeyedLocker.LockAsync("test", 0))
        {
            Assert.True(myLock.EnteredSemaphore);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public async Task TestTimeoutOrNullBasic()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (var myLock = await asyncKeyedLocker.LockOrNullAsync("test", 0))
        {
            Assert.NotNull(myLock);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutBasicWithOutParameter()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (var myLock = asyncKeyedLocker.Lock("test", 0, out var entered))
        {
            Assert.True(entered);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
            using (asyncKeyedLocker.Lock("test", 0, out entered))
                Assert.False(entered);
            using (asyncKeyedLocker.Lock("test", TimeSpan.Zero, out entered))
                Assert.False(entered);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullBasicWithOutParameter()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (var myLock = asyncKeyedLocker.LockOrNull("test", 0))
        {
            Assert.NotNull(myLock);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
            var entered = asyncKeyedLocker.LockOrNull("test", 0);
            Assert.Null(entered);
            entered = asyncKeyedLocker.LockOrNull("test", TimeSpan.Zero);
            Assert.Null(entered);
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public async Task TestTimeout()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (await asyncKeyedLocker.LockAsync("test"))
        {
            using (var myLock = await asyncKeyedLocker.LockAsync("test", 0))
            {
                Assert.False(myLock.EnteredSemaphore);
            }
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public async Task TestTimeoutOrNull()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (await asyncKeyedLocker.LockAsync("test"))
        {
            using (var myLock = await asyncKeyedLocker.LockOrNullAsync("test", 0))
            {
                Assert.Null(myLock);
            }
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithTimeSpanSynchronous()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (asyncKeyedLocker.Lock("test"))
        {
            using (asyncKeyedLocker.Lock("test", TimeSpan.Zero, out bool entered))
            {
                Assert.False(entered);
            }
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullWithTimeSpanSynchronous()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (asyncKeyedLocker.Lock("test"))
        {
            using (var result = asyncKeyedLocker.LockOrNull("test", TimeSpan.Zero))
            {
                Assert.Null(result);
            }
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithInfiniteTimeoutSynchronous()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (asyncKeyedLocker.Lock("test", Timeout.Infinite, out bool entered))
        {
            Assert.True(entered);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullWithInfiniteTimeoutSynchronous()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (var result = asyncKeyedLocker.LockOrNull("test", Timeout.Infinite))
        {
            Assert.NotNull(result);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithInfiniteTimeSpanSynchronous()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (asyncKeyedLocker.Lock("test", TimeSpan.FromMilliseconds(Timeout.Infinite), out bool entered))
        {
            Assert.True(entered);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullWithInfiniteTimeSpanSynchronous()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (var result = asyncKeyedLocker.LockOrNull("test", TimeSpan.FromMilliseconds(Timeout.Infinite)))
        {
            Assert.NotNull(result);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public async Task TestTimeoutWithTimeSpan()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (await asyncKeyedLocker.LockAsync("test"))
        {
            using (var myLock = await asyncKeyedLocker.LockAsync("test", TimeSpan.Zero))
            {
                Assert.False(myLock.EnteredSemaphore);
            }
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public async Task TestTimeoutOrNullWithTimeSpan()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (await asyncKeyedLocker.LockAsync("test"))
        {
            using (var myLock = await asyncKeyedLocker.LockOrNullAsync("test", TimeSpan.Zero))
            {
                Assert.Null(myLock);
            }
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithInfiniteTimeoutAndCancellationToken()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (asyncKeyedLocker.Lock("test", Timeout.Infinite, new CancellationToken(false), out bool entered))
        {
            Assert.True(entered);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullWithInfiniteTimeoutAndCancellationToken()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (var result = asyncKeyedLocker.LockOrNull("test", Timeout.Infinite, new CancellationToken(false)))
        {
            Assert.NotNull(result);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithZeroTimeoutAndCancellationToken()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (asyncKeyedLocker.Lock("test", 0, new CancellationToken(false), out bool entered))
        {
            Assert.True(entered);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
            using (asyncKeyedLocker.Lock("test", 0, new CancellationToken(false), out entered))
                Assert.False(entered);
            using (asyncKeyedLocker.Lock("test", TimeSpan.Zero, new CancellationToken(false), out entered))
                Assert.False(entered);
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullWithZeroTimeoutAndCancellationToken()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (var result = asyncKeyedLocker.LockOrNull("test", 0, new CancellationToken(false)))
        {
            Assert.NotNull(result);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
            var entered = asyncKeyedLocker.LockOrNull("test", 0, new CancellationToken(false));
            Assert.Null(entered);
            entered = asyncKeyedLocker.LockOrNull("test", TimeSpan.Zero, new CancellationToken(false));
            Assert.Null(entered);
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithZeroTimeoutAndCancelledToken()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        Action action = () =>
        {
            asyncKeyedLocker.Lock("test", 0, new CancellationToken(true), out bool entered);
        };
        action.Should().Throw<OperationCanceledException>();
        asyncKeyedLocker.IsInUse("test").Should().BeFalse();
    }

    [Fact]
    public void TestTimeoutOrNullWithZeroTimeoutAndCancelledToken()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        Action action = () =>
        {
            asyncKeyedLocker.LockOrNull("test", 0, new CancellationToken(true));
        };
        action.Should().Throw<OperationCanceledException>();
        asyncKeyedLocker.IsInUse("test").Should().BeFalse();
    }

    [Fact]
    public void TestTimeoutWithInfiniteTimeSpanAndCancellationToken()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (asyncKeyedLocker.Lock("test", TimeSpan.FromMilliseconds(Timeout.Infinite), new CancellationToken(false), out bool entered))
        {
            Assert.True(entered);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullWithInfiniteTimeSpanAndCancellationToken()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (var result = asyncKeyedLocker.LockOrNull("test", TimeSpan.FromMilliseconds(Timeout.Infinite), new CancellationToken(false)))
        {
            Assert.NotNull(result);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithZeroTimeSpanAndCancellationToken()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (asyncKeyedLocker.Lock("test", TimeSpan.FromMilliseconds(0), new CancellationToken(false), out bool entered))
        {
            Assert.True(entered);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullWithZeroTimeSpanAndCancellationToken()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (var result = asyncKeyedLocker.LockOrNull("test", TimeSpan.FromMilliseconds(0), new CancellationToken(false)))
        {
            Assert.NotNull(result);
            Assert.True(asyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithZeroTimeSpanAndCancelledToken()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        Action action = () =>
        {
            asyncKeyedLocker.Lock("test", TimeSpan.FromMilliseconds(0), new CancellationToken(true), out bool entered);
        };
        action.Should().Throw<OperationCanceledException>();
        asyncKeyedLocker.IsInUse("test").Should().BeFalse();
    }

    [Fact]
    public void TestTimeoutOrNullWithZeroTimeSpanAndCancelledToken()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        Action action = () =>
        {
            asyncKeyedLocker.LockOrNull("test", TimeSpan.FromMilliseconds(0), new CancellationToken(true));
        };
        action.Should().Throw<OperationCanceledException>();
        asyncKeyedLocker.IsInUse("test").Should().BeFalse();
    }

    [Fact]
    public void TestTimeoutTryLock()
    {
        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
        using (asyncKeyedLocker.Lock("test"))
        {
            Assert.True(asyncKeyedLocker.IsInUse("test"));
            Assert.False(asyncKeyedLocker.TryLock("test", () => { }, 0, CancellationToken.None));
            Assert.False(asyncKeyedLocker.TryLock("test", () => { }, TimeSpan.Zero, CancellationToken.None));
        }
        Assert.False(asyncKeyedLocker.IsInUse("test"));
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

        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
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
            var obj = await asyncKeyedLocker.LockAsync(Key);

            _ = Task.Run(async () =>
            {
                await Task.Delay(1000);
                obj.Dispose();
            });

            await asyncKeyedLocker.TryLockAsync(Key, Callback, 5000, continueOnCapturedContext);
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

        using var asyncKeyedLocker = new AsyncKeyedLocker<string>(o => { o.PoolSize = 0; });
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
            var obj = await asyncKeyedLocker.LockAsync(Key);

            _ = Task.Run(async () =>
            {
                await Task.Delay(1000);
                obj.Dispose();
            });

            await asyncKeyedLocker.TryLockAsync(Key, Callback, 5000, configureAwaitOptions);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }
}
