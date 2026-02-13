namespace BionicCode.Utilities.Net.Reflection.Exceptions;

using System;

/// <summary>
/// Exception thrown when the AssemblyLoadContext associated with a cache entry is not available since it had been unloaded.
/// </summary>
public class ReflectionCacheEntryAlcNotAvailableException : Exception
{
    private const string DefaultMessage = "The AssemblyLoadContext associated with the cache entry is not available since it had been unloaded.";

    public ReflectionCacheEntryAlcNotAvailableException()
        : base(DefaultMessage)
    {
    }

    public ReflectionCacheEntryAlcNotAvailableException(string? message)
        : base(message ?? DefaultMessage)
    {
    }

    public ReflectionCacheEntryAlcNotAvailableException(string? message, Exception? innerException)
        : base(message ?? DefaultMessage, innerException)
    {
    }
}
