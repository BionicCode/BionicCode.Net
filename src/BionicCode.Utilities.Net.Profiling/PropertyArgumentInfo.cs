namespace BionicCode.Utilities.Net.Profiling;

using System.Collections.Immutable;

internal readonly struct PropertyArgumentInfo
{
    public PropertyArgumentInfo(object value, ImmutableArray<object?>? indexerArguments, PropertyAccessor accessor, int argumentListIndex, bool isForIndexer)
    {
        ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessor>(accessor);
        ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny<PropertyAccessor>(accessor, [PropertyAccessor.Undefined]);
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(argumentListIndex);

        IndexerArguments = indexerArguments ?? ImmutableArray<object?>.Empty;
        if (isForIndexer
            && IndexerArguments.IsEmpty)
        {
            throw new ArgumentException($"The argument '{nameof(indexerArguments)}' must be provided and contain at least one element when the '{nameof(isForIndexer)}' is set to 'true'.");
        }

        ArgumentExceptionAdvanced.ThrowIfTrue(
            !isForIndexer
            && !IndexerArguments.IsEmpty,
            nameof(indexerArguments),
            $"The argument '{nameof(indexerArguments)}' was provided despite the argument '{nameof(isForIndexer)}' is set to 'false'.");

        Value = value;
        Accessor = accessor;
        ArgumentListIndex = argumentListIndex;
        IsForIndexer = isForIndexer;
    }

    /// <summary>
    /// The value for a property setter.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// The indexer arguments for an indexer property getter or setter.
    /// </summary>
    public ImmutableArray<object?> IndexerArguments { get; }

    /// <summary>
    /// The property accessor this argument is associated with.
    /// </summary>
    public PropertyAccessor Accessor { get; }

    /// <summary>
    /// The index of the current argument in the argument list provided for the profiling of the property.
    /// </summary>
    public int ArgumentListIndex { get; }

    /// <summary>
    /// Gets a value indicating whether the argument is associated with an indexer property.
    /// </summary>
    public bool IsForIndexer { get; }
}
