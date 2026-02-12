namespace BionicCode.Utilities.Net.Reflection;

using System;

/// <summary>
/// The exception that is thrown when an invalid key is used with the reflection cache.
/// </summary>
/// <remarks>This exception typically indicates a programming error where a cache key does not conform to
/// the expected format or type required by the reflection caching mechanism. Catch this exception to handle cases
/// where cache key validation fails.</remarks>
[Serializable]
public class InvalidReflectionCacheKeyException : System.Exception
{
    private const string DefaultMessage = $"The key contains invalid information that don't map to an actual reflection symbol. For example, this can be the case when the {nameof(SymbolReflectionInfoCacheKeyInternal.DeclaringTypeHandle)} is invalid or {nameof(SymbolReflectionInfoCacheKeyInternal.SymbolName)} doesn't belong to the provided {nameof(SymbolReflectionInfoCacheKeyInternal.DeclaringTypeHandle)} or the generic type parameter count for a generic method was wrong.";
    /// <inheritdoc />
    public InvalidReflectionCacheKeyException()
    {
    }
    /// <inheritdoc />
    public InvalidReflectionCacheKeyException(string message = InvalidReflectionCacheKeyException.DefaultMessage) : base(message)
    {
    }
    /// <inheritdoc />
    public InvalidReflectionCacheKeyException(string message, System.Exception inner) : base(message, inner)
    {
    }

    /// <inheritdoc />
    public InvalidReflectionCacheKeyException(System.Exception inner) : base(InvalidReflectionCacheKeyException.DefaultMessage, inner)
    {
    }
}
