using FluentAssertions;
using Xunit;

namespace AsyncKeyedLock.Tests.AsyncNonKeyedLockerTests
{
    public class OriginalTests
    {
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
                using (var myLock2 = asyncNonKeyedLocker.Lock(TimeSpan.FromMilliseconds(0), out entered))
                {
                    Assert.False(entered);
                    Assert.False(((AsyncNonKeyedLockTimeoutReleaser)myLock2).EnteredSemaphore);
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
        public void TestLockAndTimeoutAndCancellationToken()
        {
            var asyncNonKeyedLocker = new AsyncNonKeyedLocker();
            Assert.Equal(0, asyncNonKeyedLocker.GetRemainingCount());
            Assert.Equal(1, asyncNonKeyedLocker.GetCurrentCount());
            using (var myLock = asyncNonKeyedLocker.Lock(TimeSpan.FromMilliseconds(0), CancellationToken.None, out bool entered))
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
                using (var myLock2 = asyncNonKeyedLocker.Lock(TimeSpan.FromMilliseconds(0), CancellationToken.None, out entered))
                {
                    Assert.False(entered);
                    Assert.False(((AsyncNonKeyedLockTimeoutReleaser)myLock2).EnteredSemaphore);
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
                using (asyncNonKeyedLocker.Lock(TimeSpan.FromMilliseconds(0), new CancellationToken(true), out entered))
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
    }
}
