# ![AsyncKeyedLock](https://raw.githubusercontent.com/MarkCiliaVincenti/AsyncKeyedLock/master/logo32.png) AsyncKeyedLock
[![GitHub Workflow Status](https://img.shields.io/github/workflow/status/MarkCiliaVincenti/AsyncKeyedLock/.NET?logo=github&style=for-the-badge)](https://actions-badge.atrox.dev/MarkCiliaVincenti/AsyncKeyedLock/goto?ref=master) [![Nuget](https://img.shields.io/nuget/v/AsyncKeyedLock?label=AsyncKeyedLock&logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/AsyncKeyedLock) [![Nuget](https://img.shields.io/nuget/dt/AsyncKeyedLock?logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/AsyncKeyedLock)

An asynchronous .NET Standard 2.0 library that allows you to lock based on a key (keyed semaphores), only allowing a defined number of concurrent threads that share the same key.

For example, if you're processing transactions, you may want to limit to only one transaction per user so that the order is maintained, but meanwhile allowing parallel processing of multiple users.

## Benchmarks
Tests show that AsyncKeyedLock is [faster than similar libraries, while consuming less memory](https://github.com/MarkCiliaVincenti/AsyncKeyedLockBenchmarks).

## Installation
The recommended means is to use [NuGet](https://www.nuget.org/packages/AsyncKeyedLock), but you could also download the source code from [here](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/releases).

## Usage
You need to start off with creating an instance of `AsyncKeyedLocker` or `AsyncKeyedLocker<T>`. The recommended way is to use the latter, which consumes less memory. The former uses `object` and may be slightly faster, but at the expense of higher memory usage.

### Dependency injection
```csharp
services.AddSingleton<IAsyncKeyedLocker, AsyncKeyedLocker>();
```

or:

```csharp
services.AddSingleton<IAsyncKeyedLocker<string>, AsyncKeyedLocker<string>>();
```

### Variable instantiation
```csharp
var asyncKeyedLocker = new AsyncKeyedLocker();
```

or:

```csharp
var asyncKeyedLocker = new AsyncKeyedLocker<string>();
```

or if you would like to set the maximum number of requests for the semaphore that can be granted concurrently (set to 1 by default):

```csharp
var asyncKeyedLocker = new AsyncKeyedLocker<string>(2);
```

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

And if for some reason you need to force release the requests in the semaphore for a key:
```csharp
asyncKeyedLocker.ForceRelease(myObject);
```

## Credits
This library was inspired by [Stephen Cleary's solution](https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key/31194647#31194647).
