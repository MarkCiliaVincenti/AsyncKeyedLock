# ![AsyncKeyedLock](https://raw.githubusercontent.com/MarkCiliaVincenti/AsyncKeyedLock/master/logo32.png)&nbsp;AsyncKeyedLock
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/MarkCiliaVincenti/AsyncKeyedLock/dotnet.yml?branch=master&logo=github&style=flat)](https://actions-badge.atrox.dev/MarkCiliaVincenti/AsyncKeyedLock/goto?ref=master) [![NuGet](https://img.shields.io/nuget/v/AsyncKeyedLock?label=NuGet&logo=nuget&style=flat)](https://www.nuget.org/packages/AsyncKeyedLock) [![NuGet](https://img.shields.io/nuget/dt/AsyncKeyedLock?logo=nuget&style=flat)](https://www.nuget.org/packages/AsyncKeyedLock) [![Codacy Grade](https://img.shields.io/codacy/grade/315c3d5a06a441bda26ffd88e705fa63?style=flat)](https://app.codacy.com/gh/MarkCiliaVincenti/AsyncKeyedLock/dashboard) [![Codecov](https://img.shields.io/codecov/c/github/MarkCiliaVincenti/AsyncKeyedLock?label=coverage&logo=codecov&style=flat)](https://app.codecov.io/gh/MarkCiliaVincenti/AsyncKeyedLock)

An asynchronous .NET library that allows you to lock based on a key (keyed semaphores), limiting concurrent threads sharing the same key to a specified number, with optional pooling for reducing memory allocations.

For example, suppose you were processing financial transactions, but while working on one account you wouldn't want to concurrently process a transaction for the same account. Of course, you could just add a normal lock, but then you can only process one transaction at a time. If you're processing a transaction for account A, you may want to also be processing a separate transaction for account B. That's where AsyncKeyedLock comes in: it allows you to lock but only if the key matches.

The library uses two very different methods for locking, `AsyncKeyedLocker` which uses an underlying `ConcurrentDictionary` that's cleaned up after use and `StripedAsyncKeyedLocker` which uses a technique called striped locking. Both have their advantages and disadvantages, and in order to help you choose you are highly recommended to read about it in the [wiki](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/wiki).

A simple non-keyed lock is also available through [`AsyncNonKeyedLocker`](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/wiki/How-to-use-AsyncNonKeyedLocker).

## Installation and usage
Using this library is straightforward. Here's a simple example for using `AsyncKeyedLocker`:
```csharp
private static readonly AsyncKeyedLocker<string> _asyncKeyedLocker = new();

...

using (await _asyncKeyedLocker.LockAsync("test123"))
{
  ...
}
```

or with timeouts:
```csharp
private static readonly AsyncKeyedLocker<string> _asyncKeyedLocker = new();
private const _timeout = 100;

...

using (var releaser = await _asyncKeyedLocker.LockOrNullAsync("test123", _timeout))
{
  if (releaser is not null)
  {
    ...
  }
}
```

This libary also supports conditional locking, whether for `AsyncKeyedLocker`, `StripedAsyncKeyedLocker` or `AsyncNonKeyedLocker`. This could provide a workaround for `reentrancy` in some scenarios for example in recursion:
```csharp
double factorial = Factorial(number);

public static double Factorial(int number, bool isFirst = true)
{
  using (await _asyncKeyedLocker.ConditionalLockAsync("test123", isFirst))
  {
    if (number == 0)
      return 1;
    return number * Factorial(number-1, false);
  }
}
```

For more help with `AsyncKeyedLocker`, or for examples with `StripedAsyncKeyedLocker` or `AsyncNonKeyedLocker` (for simple, non-keyed locking), please [take a look at our wiki](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/wiki).

## Pooling change in v7.0.0
Prior to AsyncKeyedLock 7.0.0, pooling was disabled by default and you needed to specify a pool size in order to enable it. Since v7.0.0, pooling is enabled by default with an initial fill of 1 and a pool size of 20. In order to disable pooling (not recommended), you need to now specify a pool size of 0 within AsyncKeyedLockOptions. Read more about pooling in [our wiki](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/wiki/How-to-use-AsyncKeyedLocker#pooling).

## Non-generic constructor removal in v8.0.0
As of v8.0.0, the non-generic constructors for `AsyncKeyedLocker` has been removed. These were already marked as obsolete; if you were using them, please switch to `AsyncKeyedLocker<object>`.

## Benchmarks
This library has been extensively benchmarked against several other options and [our benchmarks](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/wiki/Benchmarks) run publicly and transparently on Github Actions.

## Credits
Check out our [list of contributors](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/blob/master/CONTRIBUTORS.md)!
