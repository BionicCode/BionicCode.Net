namespace BionicCode.Utilities.Net
{
    using System;

    internal interface IDisposable2 : IDisposable
    {
        bool IsDisposed { get; }
    }
}
