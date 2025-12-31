// Copyright (c) All contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace AsyncKeyedLock;

/// <summary>
/// A disposable that does absolutely nothing.
/// </summary>
public sealed class EmptyDisposable : IDisposable
{
    /// <summary>
    /// Dispose but in reality do nothing
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
