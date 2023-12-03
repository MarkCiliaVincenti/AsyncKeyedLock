using FluentAssertions;
using Xunit;

namespace AsyncKeyedLock.Tests.AsyncKeyedLocker;

/// <summary>
/// Adapted from https://github.com/amoerie/keyed-semaphores/blob/main/KeyedSemaphores.Tests/TestsForCancellationAndTimeout.cs
/// </summary>
public class PooledTestForCancellationTokenAndTimeout
{
    [Fact]
    public void Lock_WhenCancelled_ShouldReleaseKeyedSemaphoreAndThrowOperationCanceledException()
    {
        // Arrange
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var cancelledCancellationToken = new CancellationToken(true);

        // Act
        var action = () =>
        {
            using var _ = collection.Lock("test", cancelledCancellationToken);
        };
        action.Should().Throw<OperationCanceledException>();

        // Assert
        collection.Index.Should().NotContainKey("test");
    }

    [Fact]
    public void Lock_WhenNotCancelled_ShouldReturnDisposable()
    {
        // Arrange
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var cancellationToken = default(CancellationToken);

        // Act
        var releaser = collection.Lock("test", cancellationToken);

        // Assert
        collection.Index["test"].ReferenceCount.Should().Be(1);
        releaser.Dispose();
        collection.Index.Should().NotContainKey("test");
    }

    [Fact]
    public void TryLock_WhenCancelled_ShouldReleaseKeyedSemaphoreAndThrowOperationCanceledExceptionAndNotInvokeCallback()
    {
        // Arrange
        var isLockAcquired = false;
        var isCallbackInvoked = false;
        void Callback()
        {
            isCallbackInvoked = true;
        }
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var cancelledCancellationToken = new CancellationToken(true);

        // Act
        var action = () =>
        {
            isLockAcquired = collection.TryLock("test", Callback, TimeSpan.FromMinutes(1), cancelledCancellationToken);
        };
        action.Should().Throw<OperationCanceledException>();

        // Assert
        collection.Index.Should().NotContainKey("test");
        isLockAcquired.Should().BeFalse();
        isCallbackInvoked.Should().BeFalse();
    }

    [Fact]
    public void TryLock_WhenNotCancelled_ShouldInvokeCallbackAndReturnDisposable()
    {
        // Arrange
        bool isCallbackInvoked = false;
        void Callback()
        {
            isCallbackInvoked = true;
        }
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var cancellationToken = default(CancellationToken);

        // Act
        var isLockAcquired = collection.TryLock("test", Callback, TimeSpan.FromMinutes(1), cancellationToken);

        // Assert
        collection.Index.Should().NotContainKey("test");
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task LockAsync_WhenCancelled_ShouldReleaseKeyedSemaphoreAndThrowOperationCanceledException()
    {
        // Arrange
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var cancelledCancellationToken = new CancellationToken(true);

        // Act
        var action = async () =>
        {
            using var _ = await collection.LockAsync("test", cancelledCancellationToken);
        };
        await action.Should().ThrowAsync<OperationCanceledException>();

        // Assert
        collection.Index.Should().NotContainKey("test");
    }

    [Fact]
    public async Task LockAsync_WhenNotCancelled_ShouldReturnDisposable()
    {
        // Arrange
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var cancellationToken = default(CancellationToken);

        // Act
        var releaser = await collection.LockAsync("test", cancellationToken);

        // Assert
        collection.Index["test"].ReferenceCount.Should().Be(1);
        releaser.Dispose();
        collection.Index.Should().NotContainKey("test");
    }

    [Fact]
    public async Task TryLockAsync_WithSynchronousCallback_WhenCancelled_ShouldReleaseKeyedSemaphoreAndThrowOperationCanceledExceptionAndNotInvokeCallback()
    {
        // Arrange
        bool isLockAcquired = false;
        bool isCallbackInvoked = false;
        void Callback()
        {
            isCallbackInvoked = true;
        }
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var cancelledCancellationToken = new CancellationToken(true);

        // Act
        var action = async () =>
        {
            isLockAcquired = await collection.TryLockAsync("test", Callback, TimeSpan.FromMinutes(1), cancelledCancellationToken);
        };
        await action.Should().ThrowAsync<OperationCanceledException>();

        // Assert
        collection.Index.Should().NotContainKey("test");
        isLockAcquired.Should().BeFalse();
        isCallbackInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task TryLockAsync_WithSynchronousCallback_WhenNotCancelled_ShouldInvokeCallbackAndReturnTrue()
    {
        // Arrange
        var isCallbackInvoked = false;
        void Callback()
        {
            isCallbackInvoked = true;
        }
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var cancellationToken = default(CancellationToken);

        // Act
        var isLockAcquired = await collection.TryLockAsync("test", Callback, TimeSpan.FromMinutes(1), cancellationToken);

        // Assert
        collection.Index.Should().NotContainKey("test");
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task TryLockAsync_WithAsynchronousCallback_WhenCancelled_ShouldReleaseKeyedSemaphoreAndThrowOperationCanceledExceptionAndNotInvokeCallback()
    {
        // Arrange
        bool isLockAcquired = false;
        bool isCallbackInvoked = false;

        async Task Callback()
        {
            await Task.Delay(1);
            isCallbackInvoked = true;
        }
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var cancelledCancellationToken = new CancellationToken(true);

        // Act
        var action = async () =>
        {
            isLockAcquired = await collection.TryLockAsync("test", Callback, TimeSpan.FromMinutes(1), cancelledCancellationToken);
        };
        await action.Should().ThrowAsync<OperationCanceledException>();

        // Assert
        collection.Index.Should().NotContainKey("test");
        isLockAcquired.Should().BeFalse();
        isCallbackInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task TryLockAsync_WithAsynchronousCallback_WhenNotCancelled_ShouldInvokeCallbackAndReturnTrue()
    {
        // Arrange
        var isCallbackInvoked = false;
        async Task Callback()
        {
            await Task.Delay(1);
            isCallbackInvoked = true;
        }
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var cancellationToken = default(CancellationToken);

        // Act
        var isLockAcquired = await collection.TryLockAsync("test", Callback, TimeSpan.FromMinutes(1), cancellationToken);

        // Assert
        collection.Index.Should().NotContainKey("test");
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();
    }

    [Fact]
    public void TryLock_WhenTimedOut_ShouldNotInvokeCallbackAndReturnFalse()
    {
        // Arrange
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var key = "test";
        using var _ = collection.Lock(key);
        var isCallbackInvoked = false;
        void Callback()
        {
            isCallbackInvoked = true;
        }

        // Act
        var isLockAcquired = collection.TryLock(key, Callback, TimeSpan.FromSeconds(1));

        // Assert
        isLockAcquired.Should().BeFalse();
        isCallbackInvoked.Should().BeFalse();
        collection.Index[key].ReferenceCount.Should().Be(1);
    }

    [Fact]
    public void TryLock_WhenNotTimedOut_ShouldInvokeCallbackAndReturnTrue()
    {
        // Arrange
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var key = "test";
        var isCallbackInvoked = false;
        void Callback()
        {
            isCallbackInvoked = true;
        }

        // Act
        var isLockAcquired = collection.TryLock(key, Callback, TimeSpan.FromSeconds(1));

        // Assert
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();
        collection.Index.Should().NotContainKey(key);
    }

    [Fact]
    public async Task TryLockAsync_WhenTimedOut_ShouldNotInvokeCallbackAndReturnFalse()
    {
        // Arrange
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var key = "test";
        using var _ = await collection.LockAsync(key);
        var isCallbackInvoked = false;
        void Callback()
        {
            isCallbackInvoked = true;
        }

        // Act
        var isLockAcquired = await collection.TryLockAsync(key, Callback, TimeSpan.FromSeconds(1));

        // Assert
        isLockAcquired.Should().BeFalse();
        isCallbackInvoked.Should().BeFalse();
        collection.Index[key].ReferenceCount.Should().Be(1);
    }

    [Fact]
    public async Task TryLockAsync_WhenNotTimedOut_ShouldNotInvokeCallbackAndReturnFalse()
    {
        // Arrange
        var collection = new AsyncKeyedLocker<string>(o => { o.PoolSize = 20; o.PoolInitialFill = 1; });
        var key = "test";
        var isCallbackInvoked = false;
        void Callback()
        {
            isCallbackInvoked = true;
        }

        // Act
        var isLockAcquired = await collection.TryLockAsync(key, Callback, TimeSpan.FromSeconds(1));

        // Assert
        isLockAcquired.Should().BeTrue();
        isCallbackInvoked.Should().BeTrue();
        collection.Index.Should().NotContainKey(key);
    }
}