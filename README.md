# ![AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/raw/master/logo.png =32x32) AsyncKeyedLock
[![GitHub branch checks state](https://img.shields.io/github/checks-status/MarkCiliaVincenti/AsyncKeyedLock/master?label=build&logo=github&style=for-the-badge)](https://actions-badge.atrox.dev/MarkCiliaVincenti/AsyncKeyedLock/goto?ref=master) [![Nuget](https://img.shields.io/nuget/v/AsyncKeyedLock?label=AsyncKeyedLock&logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/AsyncKeyedLock) [![Nuget](https://img.shields.io/nuget/dt/AsyncKeyedLock?logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/AsyncKeyedLock)

An asynchronous .NET Standard 2.0 library that allows you to lock based on a key (keyed semaphores), only allowing a defined number of concurrent threads that share the same key.

For example, if you're processing transactions, you may want to limit to only one transaction per user so that the order is maintained, but meanwhile allowing parallel processing of multiple users.

## Installation
The recommended means is to use [NuGet](https://www.nuget.org/packages/AsyncKeyedLock), but you could also download the source code from [here](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/releases).

## Usage
```csharp
var asyncKeyedLocker = new AsyncKeyedLocker();
using (var lockObj = await asyncKeyedLocker.LockAsync(myObject))
{
	...
}
```

You can also set the maximum number of requests for the semaphore that can be granted concurrently (set to 1 by default):
```csharp
var asyncKeyedLocker = new AsyncKeyedLocker(2);
```

If you would like to see how many concurrent requests there are for a semaphore for a given key:
```csharp
int myCount = asyncKeyedLocker.GetCount(myObject);
```

And if for some reason you need to force release the requests in the semaphore for a key:
```csharp
asyncKeyedLocker.ForceRelease(myObject);
```

You may also use Dependency Injection to inject an instance of AsyncKeyedLock.

## Credits
This library is based on [Stephen Cleary's solution](https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key/31194647#31194647).
