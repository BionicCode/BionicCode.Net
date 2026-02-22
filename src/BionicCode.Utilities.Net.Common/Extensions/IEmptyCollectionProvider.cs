namespace BionicCode.Utilities.Net;

/// <summary>
/// Provides a static property to retrieve an empty instance of the specified collection type.
/// </summary>
/// <remarks>Implementations of this interface should provide a concrete empty collection that can be used as a
/// default or placeholder.</remarks>
/// <typeparam name="TCollection">The type of the collection that this provider returns an empty instance of.</typeparam>
public interface IEmptyCollectionProvider<out TCollection>
{
    /// <summary>
    /// Gets a static instance of an empty collection of type <typeparamref name="TCollection"/>.
    /// </summary>
    /// <remarks>Use this property to obtain an empty collection without creating a new instance. This can
    /// help avoid null references and unnecessary allocations. The returned collection is guaranteed to contain no
    /// elements and should be treated as read-only.</remarks>
    /// <value>An empty collection of type <typeparamref name="TCollection"/>.</value>
    static abstract TCollection Empty { get; }
}

