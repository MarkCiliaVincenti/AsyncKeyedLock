# AsyncKeyedLock
An asynchronous .NET Standard 2.0 library that allows you to lock based on a key.

## Installation
The recommended means is to use [NuGet](https://www.nuget.org/packages/AsyncKeyedLock), but you could also download the source code from [here](https://github.com/MarkCiliaVincenti/AsyncKeyedLock/releases).

## Usage
```csharp
using (var lockObj = await AsyncDuplicateLock.LockAsync(myObject))
{
	...
}
```

## Credits
This library is based on [Stephen Cleary's solution](https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key/31194647#31194647).