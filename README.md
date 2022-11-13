# ![AsyncKeyedLock](https://raw.githubusercontent.com/MarkCiliaVincenti/AsyncKeyedLock/master/logo32.png) AsyncKeyedLock
[![GitHub Workflow Status](https://img.shields.io/github/workflow/status/MarkCiliaVincenti/AsyncKeyedLock/.NET?logo=github&style=for-the-badge)](https://actions-badge.atrox.dev/MarkCiliaVincenti/AsyncKeyedLock/goto?ref=master) [![Nuget](https://img.shields.io/nuget/v/AsyncKeyedLock?label=AsyncKeyedLock&logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/AsyncKeyedLock) [![Nuget](https://img.shields.io/nuget/dt/AsyncKeyedLock?logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/AsyncKeyedLock)

An asynchronous .NET Standard 2.0 library that allows you to lock based on a key (keyed semaphores), limiting concurrent threads sharing the same key to a specified number.

For example, suppose you were processing financial transactions, but while working on one account you wouldn't want to concurrently process a transaction for the same account. Of course, you could just add a normal lock, but then you can only process one transaction at a time. If you're processing a transaction for account A, you may want to also be processing a separate transaction for account B. That's where AsyncKeyedLock comes in: it allows you to lock but only if the key matches.

## Installation
The recommended means is to use [NuGet](https://www.nuget.org/packages/AsyncKeyedLock), but you could also download the source code from [here](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/releases).

## Usage
You need to start off with creating an instance of `AsyncKeyedLocker` or `AsyncKeyedLocker<T>`. The recommended way is to use the latter, which is faster and consumes less memory. The former uses `object` and can be used to mix different types of objects.

### Dependency injection
```csharp
services.AddSingleton<AsyncKeyedLocker>();
```

or (recommended):

```csharp
services.AddSingleton<AsyncKeyedLocker<string>>();
```

### Variable instantiation
```csharp
var asyncKeyedLocker = new AsyncKeyedLocker();
```

or (recommended):

```csharp
var asyncKeyedLocker = new AsyncKeyedLocker<string>();
```

or if you would like to set the maximum number of requests for the semaphore that can be granted concurrently (set to 1 by default):

```csharp
var asyncKeyedLocker = new AsyncKeyedLocker<string>(2);
```

There are also AsyncKeyedLocker<TKey>() constructors which accept the parameters of ConcurrentDictionary, namely the concurrency level, the capacity and the IEqualityComparer<TKey> to use.

### Locking
```csharp
using (var lockObj = await asyncKeyedLocker.LockAsync(myObject))
{
	...
}
```

There are other overloaded methods for `LockAsync` which allow you to use `CancellationToken`, milliseconds timeout, `System.TimeSpan` or a combination of these. In the case of timeouts, you can also use `TryLockAsync` methods which will call a `Func<Task>` or `Action` if the timeout is not expired, whilst returning a boolean representing whether or not it waited successfully.

There are also synchronous `Lock` methods available, including out parameters for checking whether or not the timeout was reached.

If you would like to see how many concurrent requests there are for a semaphore for a given key:
```csharp
int myRemainingCount = asyncKeyedLocker.GetRemainingCount(myObject);
```

If you would like to see the number of remaining threads that can enter the lock for a given key:
```csharp
int myCurrentCount = asyncKeyedLocker.GetCurrentCount(myObject);
```

If you would like to check whether any request is using a specific key:
```csharp
bool isInUse = asyncKeyedLocker.IsInUse(myObject);
```

## Credits
This library was inspired by [Stephen Cleary's solution](https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key/31194647#31194647).
