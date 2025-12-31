using AsyncKeyedLock.Tests.Helpers;
using FluentAssertions;
using System.Collections;
using Xunit;

namespace AsyncKeyedLock.Tests.StripedAsyncKeyedLocker;

[Collection("Original Tests")]
[CollectionDefinition("Original Tests", DisableParallelization = false)]
public class OriginalTests
{
    [Fact]
    public void ConcurrentDispose_ShouldNotThrow()
    {
        Action action = () =>
        {
            var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
            asyncKeyedLocker.Dispose();
            asyncKeyedLocker.Dispose();
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestRecursion()
    {
        using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

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
        using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

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
        using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

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
        using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

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
        using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

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
        using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

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
        using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

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
        using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

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
        using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

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
        using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

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
        using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

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
        using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>();

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
            using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>(0);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestHashHelpersIsPrime1DoesNotThrow()
    {
        Action action = () =>
        {
            using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>(1);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestHashHelpersIsPrime7199374DoesNotThrow()
    {
        Action action = () =>
        {
            using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>(7199374);
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
            using var asyncKeyedLocker = new StripedAsyncKeyedLocker<string>(-1);
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
            using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(comparer: EqualityComparer<string>.Default);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestComparerAndMaxCount1ShouldBePossible()
    {
        Action action = () =>
        {
            using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(maxCount: 1, comparer: EqualityComparer<string>.Default);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestComparerAndMaxCount0ShouldNotBePossible()
    {
        Action action = () =>
        {
            using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(maxCount: 0, comparer: EqualityComparer<string>.Default);
        };
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TestNumberOfStripesShouldBePossible()
    {
        Action action = () =>
        {
            using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(Environment.ProcessorCount);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void TestMaxCount0WithNumberOfStripesShouldNotBePossible()
    {
        Action action = () =>
        {
            using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(numberOfStripes: 42, maxCount: 0);
        };
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TestReadingMaxCount()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(maxCount: 2);
        stripedAsyncKeyedLocker.MaxCount.Should().Be(2);
    }

    [Fact]
    public void TestReadingMaxCountViaParameterWithComparer()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(maxCount: 2, comparer: EqualityComparer<string>.Default);
        stripedAsyncKeyedLocker.MaxCount.Should().Be(2);
    }

    [Fact]
    public void TestReadingMaxCountViaParameterWithNumberOfStripes()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(42, 2);
        stripedAsyncKeyedLocker.MaxCount.Should().Be(2);
    }

    [Fact]
    public void TestReadingMaxCountViaParameterWithNumberOfStripesAndComparer()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>(42, 2, EqualityComparer<string>.Default);
        stripedAsyncKeyedLocker.MaxCount.Should().Be(2);
    }

    [Fact]
    public async Task TestTimeoutBasic()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (var myLock = await stripedAsyncKeyedLocker.LockAsync("test", 0))
        {
            Assert.True(myLock.EnteredSemaphore);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public async Task TestTimeoutOrNullBasic()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (var myLock = await stripedAsyncKeyedLocker.LockOrNullAsync("test", 0))
        {
            Assert.NotNull(myLock);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutBasicWithOutParameter()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
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
    public void TestTimeoutOrNullBasicWithOutParameter()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (var myLock = stripedAsyncKeyedLocker.LockOrNull("test", 0))
        {
            Assert.NotNull(myLock);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
            var entered = stripedAsyncKeyedLocker.LockOrNull("test", 0);
            Assert.Null(entered);
            entered = stripedAsyncKeyedLocker.LockOrNull("test", TimeSpan.Zero);
            Assert.Null(entered);
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public async Task TestTimeout()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
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
    public async Task TestTimeoutOrNull()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (await stripedAsyncKeyedLocker.LockAsync("test"))
        {
            using (var myLock = await stripedAsyncKeyedLocker.LockOrNullAsync("test", 0))
            {
                Assert.Null(myLock);
            }
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithTimeSpanSynchronous()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
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
    public void TestTimeoutOrNullWithTimeSpanSynchronous()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (stripedAsyncKeyedLocker.Lock("test"))
        {
            using (var result = stripedAsyncKeyedLocker.LockOrNull("test", TimeSpan.Zero))
            {
                Assert.Null(result);
            }
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithInfiniteTimeoutSynchronous()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (stripedAsyncKeyedLocker.Lock("test", Timeout.Infinite, out bool entered))
        {
            Assert.True(entered);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullWithInfiniteTimeoutSynchronous()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (var result = stripedAsyncKeyedLocker.LockOrNull("test", Timeout.Infinite))
        {
            Assert.NotNull(result);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithInfiniteTimeSpanSynchronous()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (stripedAsyncKeyedLocker.Lock("test", TimeSpan.FromMilliseconds(Timeout.Infinite), out bool entered))
        {
            Assert.True(entered);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullWithInfiniteTimeSpanSynchronous()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (var result = stripedAsyncKeyedLocker.LockOrNull("test", TimeSpan.FromMilliseconds(Timeout.Infinite)))
        {
            Assert.NotNull(result);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public async Task TestTimeoutWithTimeSpan()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
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
    public async Task TestTimeoutOrNullWithTimeSpan()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (await stripedAsyncKeyedLocker.LockAsync("test"))
        {
            using (var myLock = await stripedAsyncKeyedLocker.LockOrNullAsync("test", TimeSpan.Zero))
            {
                Assert.Null(myLock);
            }
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithInfiniteTimeoutAndCancellationToken()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (stripedAsyncKeyedLocker.Lock("test", Timeout.Infinite, new CancellationToken(false), out bool entered))
        {
            Assert.True(entered);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullWithInfiniteTimeoutAndCancellationToken()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (var result = stripedAsyncKeyedLocker.LockOrNull("test", Timeout.Infinite, new CancellationToken(false)))
        {
            Assert.NotNull(result);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithZeroTimeoutAndCancellationToken()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
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
    public void TestTimeoutOrNullWithZeroTimeoutAndCancellationToken()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (var result = stripedAsyncKeyedLocker.LockOrNull("test", 0, new CancellationToken(false)))
        {
            Assert.NotNull(result);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
            var entered = stripedAsyncKeyedLocker.LockOrNull("test", 0, new CancellationToken(false));
            Assert.Null(entered);
            entered = stripedAsyncKeyedLocker.LockOrNull("test", TimeSpan.Zero, new CancellationToken(false));
            Assert.Null(entered);
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithZeroTimeoutAndCancelledToken()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        Action action = () =>
        {
            stripedAsyncKeyedLocker.Lock("test", 0, new CancellationToken(true), out bool entered);
        };
        action.Should().Throw<OperationCanceledException>();
        stripedAsyncKeyedLocker.IsInUse("test").Should().BeFalse();
    }

    [Fact]
    public void TestTimeoutOrNullWithZeroTimeoutAndCancelledToken()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        Action action = () =>
        {
            stripedAsyncKeyedLocker.LockOrNull("test", 0, new CancellationToken(true));
        };
        action.Should().Throw<OperationCanceledException>();
        stripedAsyncKeyedLocker.IsInUse("test").Should().BeFalse();
    }

    [Fact]
    public void TestTimeoutWithInfiniteTimeSpanAndCancellationToken()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (stripedAsyncKeyedLocker.Lock("test", TimeSpan.FromMilliseconds(Timeout.Infinite), new CancellationToken(false), out bool entered))
        {
            Assert.True(entered);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullWithInfiniteTimeSpanAndCancellationToken()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (var result = stripedAsyncKeyedLocker.LockOrNull("test", TimeSpan.FromMilliseconds(Timeout.Infinite), new CancellationToken(false)))
        {
            Assert.NotNull(result);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithZeroTimeSpanAndCancellationToken()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (stripedAsyncKeyedLocker.Lock("test", TimeSpan.FromMilliseconds(0), new CancellationToken(false), out bool entered))
        {
            Assert.True(entered);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutOrNullWithZeroTimeSpanAndCancellationToken()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (var result = stripedAsyncKeyedLocker.LockOrNull("test", TimeSpan.FromMilliseconds(0), new CancellationToken(false)))
        {
            Assert.NotNull(result);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
    }

    [Fact]
    public void TestTimeoutWithZeroTimeSpanAndCancelledToken()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        Action action = () =>
        {
            stripedAsyncKeyedLocker.Lock("test", TimeSpan.FromMilliseconds(0), new CancellationToken(true), out bool entered);
        };
        action.Should().Throw<OperationCanceledException>();
        stripedAsyncKeyedLocker.IsInUse("test").Should().BeFalse();
    }

    [Fact]
    public void TestTimeoutOrNullWithZeroTimeSpanAndCancelledToken()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        Action action = () =>
        {
            stripedAsyncKeyedLocker.LockOrNull("test", TimeSpan.FromMilliseconds(0), new CancellationToken(true));
        };
        action.Should().Throw<OperationCanceledException>();
        stripedAsyncKeyedLocker.IsInUse("test").Should().BeFalse();
    }

    [Fact]
    public void TestTimeoutTryLock()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
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
    public void TestTimeoutOrNullTryLock()
    {
        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
        using (var result = stripedAsyncKeyedLocker.LockOrNull("test", TimeSpan.Zero))
        {
            Assert.NotNull(result);
            Assert.True(stripedAsyncKeyedLocker.IsInUse("test"));
            Assert.False(stripedAsyncKeyedLocker.TryLock("test", () => { }, 0, CancellationToken.None));
            Assert.False(stripedAsyncKeyedLocker.TryLock("test", () => { }, TimeSpan.Zero, CancellationToken.None));
        }
        Assert.False(stripedAsyncKeyedLocker.IsInUse("test"));
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

        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
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
            var obj = await stripedAsyncKeyedLocker.LockAsync(Key);

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

        using var stripedAsyncKeyedLocker = new StripedAsyncKeyedLocker<string>();
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
            var obj = await stripedAsyncKeyedLocker.LockAsync(Key);

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
