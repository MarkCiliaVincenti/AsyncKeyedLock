using FluentAssertions;
using Xunit;

namespace AsyncKeyedLock.Tests.AsyncNonKeyedLockerTests
{
    [Collection("NonKeyed Tests")]
    [CollectionDefinition("NonKeyed Tests", DisableParallelization = false)]
    public class OriginalTests
    {
        [Fact]
        public void TestRecursion()
        {
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();

            double Factorial(int number, bool isFirst = true)
            {
                using (asyncNonKeyedLocker.ConditionalLock(isFirst))
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
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();

            async Task<double> Factorial(int number, bool isFirst = true)
            {
                using (await asyncNonKeyedLocker.ConditionalLockAsync(isFirst))
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
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();

            double Factorial(int number, bool isFirst = true)
            {
                using (asyncNonKeyedLocker.ConditionalLock(isFirst, new CancellationToken(false)))
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
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();

            async Task<double> Factorial(int number, bool isFirst = true)
            {
                using (await asyncNonKeyedLocker.ConditionalLockAsync(isFirst, new CancellationToken(false)))
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
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();

            double Factorial(int number, bool isFirst = true)
            {
                using (asyncNonKeyedLocker.ConditionalLock(isFirst, Timeout.Infinite))
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
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();

            async Task<double> Factorial(int number, bool isFirst = true)
            {
                using (await asyncNonKeyedLocker.ConditionalLockAsync(isFirst, Timeout.Infinite))
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
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();

            double Factorial(int number, bool isFirst = true)
            {
                using (asyncNonKeyedLocker.ConditionalLock(isFirst, TimeSpan.Zero))
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
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();

            async Task<double> Factorial(int number, bool isFirst = true)
            {
                using (await asyncNonKeyedLocker.ConditionalLockAsync(isFirst, TimeSpan.Zero))
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
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();

            double Factorial(int number, bool isFirst = true)
            {
                using (asyncNonKeyedLocker.ConditionalLock(isFirst, Timeout.Infinite, new CancellationToken(false)))
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
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();

            async Task<double> Factorial(int number, bool isFirst = true)
            {
                using (await asyncNonKeyedLocker.ConditionalLockAsync(isFirst, Timeout.Infinite, new CancellationToken(false)))
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
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();

            double Factorial(int number, bool isFirst = true)
            {
                using (asyncNonKeyedLocker.ConditionalLock(isFirst, TimeSpan.Zero, new CancellationToken(false)))
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
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();

            async Task<double> Factorial(int number, bool isFirst = true)
            {
                using (await asyncNonKeyedLocker.ConditionalLockAsync(isFirst, TimeSpan.Zero, new CancellationToken(false)))
                {
                    if (number == 0)
                        return 1;
                    return number * await Factorial(number - 1, false);
                }
            }

            Assert.Equal(120, await Factorial(5));
        }

        [Fact]
        public void TestMaxCount()
        {
            using var asyncNonKeyedLocker = new AsyncNonKeyedLocker(2);
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(asyncNonKeyedLocker.MaxCount, asyncNonKeyedLocker.GetCurrentCount());
            using (var myLock = (AsyncNonKeyedLockReleaser)asyncNonKeyedLocker.Lock())
            {
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
                using (var myLock2 = (AsyncNonKeyedLockReleaser)asyncNonKeyedLocker.Lock())
                {
                    Assert.Equal(asyncNonKeyedLocker.MaxCount, asyncNonKeyedLocker.GetRemainingCount());
                    Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
                }
            }
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(asyncNonKeyedLocker.MaxCount, asyncNonKeyedLocker.GetCurrentCount());
        }

        [Fact]
        public void TestLock()
        {
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
            using (var myLock = (AsyncNonKeyedLockReleaser)asyncNonKeyedLocker.Lock())
            {
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
            }
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
        }

        [Fact]
        public void TestLockAndCancellationToken()
        {
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
            using (asyncNonKeyedLocker.Lock(CancellationToken.None))
            {
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
            }
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
        }

        [Fact]
        public void TestLockAndCancelledCancellationToken()
        {
            Action action = () =>
            {
                var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
                using (asyncNonKeyedLocker.Lock(new CancellationToken(true)))
                { }
            };
            action.Should().Throw<OperationCanceledException>();
        }

        [Fact]
        public void TestLockAndMillisecondsTimeout()
        {
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
            using (var myLock = asyncNonKeyedLocker.Lock(Timeout.Infinite, out bool entered))
            {
                Assert.True(entered);
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
                using (var myLock2 = asyncNonKeyedLocker.Lock(0, out entered))
                {
                    Assert.False(entered);
                    Assert.False(((AsyncNonKeyedLockTimeoutReleaser)myLock2).EnteredSemaphore);
                }
            }
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
        }

        [Fact]
        public void TestLockOrNullAndMillisecondsTimeout()
        {
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
            using (var myLock = asyncNonKeyedLocker.LockOrNull(Timeout.Infinite))
            {
                Assert.NotNull(myLock);
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
                using (var myLock2 = asyncNonKeyedLocker.LockOrNull(0))
                {
                    Assert.Null(myLock2);
                }
            }
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
        }

        [Fact]
        public void TestLockAndTimeout()
        {
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
            using (var myLock = asyncNonKeyedLocker.Lock(Timeout.InfiniteTimeSpan, out bool entered))
            {
                Assert.True(entered);
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
                using (var myLock2 = asyncNonKeyedLocker.Lock(TimeSpan.Zero, out entered))
                {
                    Assert.False(entered);
                    Assert.False(((AsyncNonKeyedLockTimeoutReleaser)myLock2).EnteredSemaphore);
                }
            }
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
        }

        [Fact]
        public void TestLockOrNullAndTimeout()
        {
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
            using (var myLock = asyncNonKeyedLocker.LockOrNull(Timeout.InfiniteTimeSpan))
            {
                Assert.NotNull(myLock);
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
                using (var myLock2 = asyncNonKeyedLocker.LockOrNull(TimeSpan.Zero))
                {
                    Assert.Null(myLock2);
                }
            }
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
        }

        [Fact]
        public void TestLockAndMillisecondsTimeoutAndCancellationToken()
        {
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
            using (var myLock = asyncNonKeyedLocker.Lock(0, CancellationToken.None, out bool entered))
            {
                Assert.True(entered);
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
            }
            using (var myLock = asyncNonKeyedLocker.Lock(Timeout.Infinite, CancellationToken.None, out bool entered))
            {
                Assert.True(entered);
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
                using (var myLock2 = asyncNonKeyedLocker.Lock(0, CancellationToken.None, out entered))
                {
                    Assert.False(entered);
                    Assert.False(((AsyncNonKeyedLockTimeoutReleaser)myLock2).EnteredSemaphore);
                }
            }
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
        }

        [Fact]
        public void TestLockOrNullAndMillisecondsTimeoutAndCancellationToken()
        {
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
            using (var myLock = asyncNonKeyedLocker.LockOrNull(0, CancellationToken.None))
            {
                Assert.NotNull(myLock);
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
            }
            using (var myLock = asyncNonKeyedLocker.LockOrNull(Timeout.Infinite, CancellationToken.None))
            {
                Assert.NotNull(myLock);
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
                using (var myLock2 = asyncNonKeyedLocker.LockOrNull(0, CancellationToken.None))
                {
                    Assert.Null(myLock2);
                }
            }
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
        }

        [Fact]
        public void TestLockAndMillisecondsTimeoutAndCancelledCancellationToken()
        {
            bool entered = false;
            Action action = () =>
            {
                var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
                using (asyncNonKeyedLocker.Lock(0, new CancellationToken(true), out entered))
                { }
            };
            action.Should().Throw<OperationCanceledException>();
            entered.Should().BeFalse();
        }

        [Fact]
        public void TestLockOrNullAndMillisecondsTimeoutAndCancelledCancellationToken()
        {
            bool entered = false;
            Action action = () =>
            {
                var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
                using (asyncNonKeyedLocker.LockOrNull(0, new CancellationToken(true)))
                { }
            };
            action.Should().Throw<OperationCanceledException>();
            entered.Should().BeFalse();
        }

        [Fact]
        public void TestLockAndInfiniteMillisecondsTimeoutAndCancelledCancellationToken()
        {
            bool entered = false;
            Action action = () =>
            {
                var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
                using (asyncNonKeyedLocker.Lock(Timeout.Infinite, new CancellationToken(true), out entered))
                { }
            };
            action.Should().Throw<OperationCanceledException>();
            entered.Should().BeFalse();
        }

        [Fact]
        public void TestLockOrNullAndInfiniteMillisecondsTimeoutAndCancelledCancellationToken()
        {
            bool entered = false;
            Action action = () =>
            {
                var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
                using (asyncNonKeyedLocker.LockOrNull(Timeout.Infinite, new CancellationToken(true)))
                { }
            };
            action.Should().Throw<OperationCanceledException>();
            entered.Should().BeFalse();
        }

        [Fact]
        public void TestLockAndTimeoutAndCancellationToken()
        {
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
            using (var myLock = asyncNonKeyedLocker.Lock(TimeSpan.Zero, CancellationToken.None, out bool entered))
            {
                Assert.True(entered);
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
            }
            using (var myLock = asyncNonKeyedLocker.Lock(Timeout.InfiniteTimeSpan, CancellationToken.None, out bool entered))
            {
                Assert.True(entered);
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
                using (var myLock2 = asyncNonKeyedLocker.Lock(TimeSpan.Zero, CancellationToken.None, out entered))
                {
                    Assert.False(entered);
                    Assert.False(((AsyncNonKeyedLockTimeoutReleaser)myLock2).EnteredSemaphore);
                }
            }
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
        }

        [Fact]
        public void TestLockOrNullAndTimeoutAndCancellationToken()
        {
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
            using (var myLock = asyncNonKeyedLocker.LockOrNull(TimeSpan.Zero, CancellationToken.None))
            {
                Assert.NotNull(myLock);
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
            }
            using (var myLock = asyncNonKeyedLocker.LockOrNull(Timeout.InfiniteTimeSpan, CancellationToken.None))
            {
                Assert.NotNull(myLock);
                Assert.Equal(1, asyncNonKeyedLocker.GetRemainingCount());
                Assert.Equal(0, asyncNonKeyedLocker.GetCurrentCount());
                using (var myLock2 = asyncNonKeyedLocker.LockOrNull(TimeSpan.Zero, CancellationToken.None))
                {
                    Assert.Null(myLock2);
                }
            }
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
        }

        [Fact]
        public void TestLockAndTimeoutAndCancelledCancellationToken()
        {
            bool entered = false;
            Action action = () =>
            {
                var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
                using (asyncNonKeyedLocker.Lock(TimeSpan.Zero, new CancellationToken(true), out entered))
                { }
            };
            action.Should().Throw<OperationCanceledException>();
            entered.Should().BeFalse();
        }

        [Fact]
        public void TestLockOrNullAndTimeoutAndCancelledCancellationToken()
        {
            bool entered = false;
            Action action = () =>
            {
                var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
                using (asyncNonKeyedLocker.LockOrNull(TimeSpan.Zero, new CancellationToken(true)))
                { }
            };
            action.Should().Throw<OperationCanceledException>();
            entered.Should().BeFalse();
        }

        [Fact]
        public void TestLockAndInfiniteTimeoutAndCancelledCancellationToken()
        {
            bool entered = false;
            Action action = () =>
            {
                var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
                using (asyncNonKeyedLocker.Lock(Timeout.InfiniteTimeSpan, new CancellationToken(true), out entered))
                { }
            };
            action.Should().Throw<OperationCanceledException>();
            entered.Should().BeFalse();
        }

        [Fact]
        public void TestLockOrNullAndInfiniteTimeoutAndCancelledCancellationToken()
        {
            bool entered = false;
            Action action = () =>
            {
                var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
                using (asyncNonKeyedLocker.LockOrNull(Timeout.InfiniteTimeSpan, new CancellationToken(true)))
                { }
            };
            action.Should().Throw<OperationCanceledException>();
            entered.Should().BeFalse();
        }
    }
}
