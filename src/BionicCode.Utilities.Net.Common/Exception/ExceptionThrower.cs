namespace BionicCode.Utilities.Net;

using System;
using System.Diagnostics.CodeAnalysis;

public static class ExceptionThrower
{
    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if the specified disposable object has already been disposed.
    /// </summary>
    /// <typeparam name="TDisposable">The type of the disposable object.</typeparam>
    /// <param name="disposable">The disposable object to check.</param>
    /// <exception cref="ObjectDisposedException">The disposable object <paramref name="disposable"/> has been disposed.</exception>
    public static void ThrowIfDisposed<TDisposable>([NotNull] TDisposable disposable)
        where TDisposable : IDisposableAdvanced
    {
        ArgumentNullException.ThrowIfNull(disposable);

        if (disposable.IsDisposed)
        {
            throw new ObjectDisposedException(nameof(disposable), ExceptionMessages.GetObjectDisposedExceptionMessage(disposable.GetType().ToFullyQualifiedSignatureName()));
        }
    }
}
