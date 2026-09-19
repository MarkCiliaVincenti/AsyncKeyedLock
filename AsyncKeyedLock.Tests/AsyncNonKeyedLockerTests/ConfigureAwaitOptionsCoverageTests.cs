using FluentAssertions;
using Xunit;

namespace AsyncKeyedLock.Tests.AsyncNonKeyedLockerTests;

public class ConfigureAwaitOptionsCoverageTests
{
    [Fact]
    public async Task ConfigureAwaitOptions_LockAndLockOrNullOverloads_ShouldExecute()
    {
        using var locker = new AsyncNonKeyedLocker();

        using (await locker.LockAsync(ConfigureAwaitOptions.None)) { }
        using (await locker.LockAsync(CancellationToken.None, ConfigureAwaitOptions.None)) { }

        using (var timeout = await locker.LockAsync(0, ConfigureAwaitOptions.None))
        {
            timeout.EnteredSemaphore.Should().BeTrue();
        }

        using (var timeout = await locker.LockAsync(TimeSpan.Zero, ConfigureAwaitOptions.None))
        {
            timeout.EnteredSemaphore.Should().BeTrue();
        }

        using (var cancelled = await locker.LockAsync(0, new CancellationToken(true), ConfigureAwaitOptions.None))
        {
            cancelled.EnteredSemaphore.Should().BeFalse();
        }

        using (var cancelled = await locker.LockAsync(TimeSpan.Zero, new CancellationToken(true), ConfigureAwaitOptions.None))
        {
            cancelled.EnteredSemaphore.Should().BeFalse();
        }

        var intLock = await locker.LockOrNullAsync(0, ConfigureAwaitOptions.None);
        intLock.Should().NotBeNull();
        intLock!.Dispose();

        var spanLock = await locker.LockOrNullAsync(TimeSpan.Zero, ConfigureAwaitOptions.None);
        spanLock.Should().NotBeNull();
        spanLock!.Dispose();

        Func<Task> cancelledInt = async () => await locker.LockOrNullAsync(0, new CancellationToken(true), ConfigureAwaitOptions.None);
        Func<Task> cancelledSpan = async () => await locker.LockOrNullAsync(TimeSpan.Zero, new CancellationToken(true), ConfigureAwaitOptions.None);
        await cancelledInt.Should().ThrowAsync<OperationCanceledException>();
        await cancelledSpan.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ConfigureAwaitOptions_ConditionalLockOverloads_ShouldHonorCondition()
    {
        using var locker = new AsyncNonKeyedLocker();

        (await locker.ConditionalLockAsync(false, ConfigureAwaitOptions.None)).Should().BeNull();
        using (await locker.ConditionalLockAsync(true, ConfigureAwaitOptions.None)) { }
        (await locker.ConditionalLockAsync(false, CancellationToken.None, ConfigureAwaitOptions.None)).Should().BeNull();
        using (await locker.ConditionalLockAsync(true, CancellationToken.None, ConfigureAwaitOptions.None)) { }
        (await locker.ConditionalLockAsync(false, 0, ConfigureAwaitOptions.None)).Should().BeNull();
        using (await locker.ConditionalLockAsync(true, 0, ConfigureAwaitOptions.None)) { }
        (await locker.ConditionalLockAsync(false, TimeSpan.Zero, ConfigureAwaitOptions.None)).Should().BeNull();
        using (await locker.ConditionalLockAsync(true, TimeSpan.Zero, ConfigureAwaitOptions.None)) { }
        (await locker.ConditionalLockAsync(false, 0, CancellationToken.None, ConfigureAwaitOptions.None)).Should().BeNull();
        using (await locker.ConditionalLockAsync(true, 0, CancellationToken.None, ConfigureAwaitOptions.None)) { }
        (await locker.ConditionalLockAsync(false, TimeSpan.Zero, CancellationToken.None, ConfigureAwaitOptions.None)).Should().BeNull();
        using (await locker.ConditionalLockAsync(true, TimeSpan.Zero, CancellationToken.None, ConfigureAwaitOptions.None)) { }
    }
}
