using FluentAssertions;
using Xunit;

namespace AsyncKeyedLock.Tests.AsyncKeyedLocker;

public class ConfigureAwaitOptionsCoverageTests
{
    [Fact]
    public async Task ConfigureAwaitOptions_LockOverloads_ShouldExecute()
    {
        using var locker = new AsyncKeyedLocker<string>();

        using (await locker.LockAsync("lock", ConfigureAwaitOptions.None)) { }
        using (await locker.LockAsync("lock-cancel", CancellationToken.None, ConfigureAwaitOptions.None)) { }

        using (var timeout = await locker.LockAsync("timeout-int", 0, ConfigureAwaitOptions.None))
        {
            timeout.EnteredSemaphore.Should().BeTrue();
        }

        using (var timeout = await locker.LockAsync("timeout-span", TimeSpan.Zero, ConfigureAwaitOptions.None))
        {
            timeout.EnteredSemaphore.Should().BeTrue();
        }

        Func<Task> cancelledInt = async () => await locker.LockAsync("cancel-int", 0, new CancellationToken(true), ConfigureAwaitOptions.None);
        Func<Task> cancelledSpan = async () => await locker.LockAsync("cancel-span", TimeSpan.Zero, new CancellationToken(true), ConfigureAwaitOptions.None);
        await cancelledInt.Should().ThrowAsync<OperationCanceledException>();
        await cancelledSpan.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ConfigureAwaitOptions_LockOrNullOverloads_ShouldExecute()
    {
        using var locker = new AsyncKeyedLocker<string>();

        var intLock = await locker.LockOrNullAsync("int", 0, ConfigureAwaitOptions.None);
        intLock.Should().NotBeNull();
        intLock!.Dispose();

        var spanLock = await locker.LockOrNullAsync("span", TimeSpan.Zero, ConfigureAwaitOptions.None);
        spanLock.Should().NotBeNull();
        spanLock!.Dispose();

        Func<Task> cancelledInt = async () => await locker.LockOrNullAsync("cancel-int", 0, new CancellationToken(true), ConfigureAwaitOptions.None);
        Func<Task> cancelledSpan = async () => await locker.LockOrNullAsync("cancel-span", TimeSpan.Zero, new CancellationToken(true), ConfigureAwaitOptions.None);
        await cancelledInt.Should().ThrowAsync<OperationCanceledException>();
        await cancelledSpan.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ConfigureAwaitOptions_ConditionalLockOverloads_ShouldHonorCondition()
    {
        using var locker = new AsyncKeyedLocker<string>();

        (await locker.ConditionalLockAsync("plain-false", false, ConfigureAwaitOptions.None)).Should().BeNull();
        using (await locker.ConditionalLockAsync("plain-true", true, ConfigureAwaitOptions.None)) { }
        (await locker.ConditionalLockAsync("token-false", false, CancellationToken.None, ConfigureAwaitOptions.None)).Should().BeNull();
        using (await locker.ConditionalLockAsync("token-true", true, CancellationToken.None, ConfigureAwaitOptions.None)) { }
        (await locker.ConditionalLockAsync("int-false", false, 0, ConfigureAwaitOptions.None)).Should().BeNull();
        using (await locker.ConditionalLockAsync("int-true", true, 0, ConfigureAwaitOptions.None)) { }
        (await locker.ConditionalLockAsync("span-false", false, TimeSpan.Zero, ConfigureAwaitOptions.None)).Should().BeNull();
        using (await locker.ConditionalLockAsync("span-true", true, TimeSpan.Zero, ConfigureAwaitOptions.None)) { }
        (await locker.ConditionalLockAsync("cancel-int-false", false, 0, new CancellationToken(true), ConfigureAwaitOptions.None)).Should().BeNull();
        (await locker.ConditionalLockAsync("cancel-span-false", false, TimeSpan.Zero, new CancellationToken(true), ConfigureAwaitOptions.None)).Should().BeNull();
    }
}
