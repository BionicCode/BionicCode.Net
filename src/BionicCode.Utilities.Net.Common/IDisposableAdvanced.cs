namespace BionicCode.Utilities.Net;

using System;

public interface IDisposableAdvanced : IDisposable
{
    bool IsDisposed { get; }
}
