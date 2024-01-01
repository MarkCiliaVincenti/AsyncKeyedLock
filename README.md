# ![AsyncKeyedLock](https://raw.githubusercontent.com/MarkCiliaVincenti/AsyncKeyedLock/master/logo32.png) AsyncKeyedLock
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/MarkCiliaVincenti/AsyncKeyedLock/dotnet.yml?branch=master&logo=github&style=flat)](https://actions-badge.atrox.dev/MarkCiliaVincenti/AsyncKeyedLock/goto?ref=master) [![NuGet](https://img.shields.io/nuget/v/AsyncKeyedLock?label=AsyncKeyedLock&logo=nuget&style=flat)](https://www.nuget.org/packages/AsyncKeyedLock) [![NuGet](https://img.shields.io/nuget/dt/AsyncKeyedLock?logo=nuget&style=flat)](https://www.nuget.org/packages/AsyncKeyedLock) [![Codacy Grade](https://img.shields.io/codacy/grade/315c3d5a06a441bda26ffd88e705fa63?style=flat)](https://app.codacy.com/gh/MarkCiliaVincenti/AsyncKeyedLock/dashboard) [![Codecov](https://img.shields.io/codecov/c/github/MarkCiliaVincenti/AsyncKeyedLock?label=Coverage&logo=codecov&style=flat)](https://app.codecov.io/gh/MarkCiliaVincenti/AsyncKeyedLock)

An asynchronous .NET Standard 2.0 library that allows you to lock based on a key (keyed semaphores), limiting concurrent threads sharing the same key to a specified number, with optional pooling for reducing memory allocations.

For example, suppose you were processing financial transactions, but while working on one account you wouldn't want to concurrently process a transaction for the same account. Of course, you could just add a normal lock, but then you can only process one transaction at a time. If you're processing a transaction for account A, you may want to also be processing a separate transaction for account B. That's where AsyncKeyedLock comes in: it allows you to lock but only if the key matches.

The library uses two very different methods for locking, one using an underlying `ConcurrentDictionary` that's cleaned up after use whilst the other using a technique called striped locking. Both have their advantages and disadvantages, and in order to help you choose you are highly recommended to read about it in the [wiki](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/wiki).

## Installation and usage
Using this library is straightforward. Here's a simple example:
```csharp
private static readonly AsyncKeyedLocker<string> _asyncKeyedLocker = new(o =>
	{
		o.PoolSize = 20; // this is NOT a concurrency limit
		o.PoolInitialFill = 1;
	});

...

using (await _asyncKeyedLocker.LockAsync("test123"))
{
	...
}
```

The documentation can be found in our [wiki](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/wiki).

Usage

## Credits
Check out our [list of contributors](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/blob/master/CONTRIBUTORS.md)!
