namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// A read-only list of <see cref="ParameterDescriptor"/> items sorted by parameter position in ascending order.
/// </summary>
/// <remarks>It is expected that all <see cref="ParameterDescriptor"/> items in the list:
/// <list type="bullet">
/// <item>have unique parameter positions or unique parameter names to avoid ambiguity when used for lookups.</item>
/// <item>don't have duplicate positions or names.</item>
/// <item>don't have a position value that exceeds the total number of parameters in the collection.</item>
/// <item>belong to the same member of the same declaring type.</item>
/// <item>represent the complete formal parameter list of the method or constructor.</item>
/// </list>
/// <para/>Ambiguity is defined as the lack of sufficient information that leads to multiple method candidates during the lookup. 
/// <para/>To avoid ambiguity and to significantly improve the accuracy and performance of lookups, it is highly recommended to construct the <see cref="ParameterDescriptor"/> instances using all available information.
/// <para/>See the <see cref="ParameterDescriptor"/> documentation for more information on how to construct the descriptors and the expected values for their properties to ensure maximum accuracy and performance when used for lookups.
/// <para/>The collection is sorted by parameter position in ascending order.
/// <para/>To ensure the integrity of the collection, the constructor validates the provided items for duplicates and consistency of declaring method parameter count when applicable. 
/// <br/>If any validation fails, an appropriate exception is thrown to indicate the specific issue with the input collection.
/// <para/>This collection is not intended for a loose collection of unrelated parameters.
/// <br/>Instead the collection is a strict representation of a method's formal parameter list.
/// <para/>The collection is immutable and thread-safe.</remarks>
public sealed class ParameterDescriptorList : IReadOnlyList<ParameterDescriptor>, IEmptyCollectionProvider<ParameterDescriptorList>, IEquatable<ParameterDescriptorList>
{
    public static ParameterDescriptorList Empty { get; } = new ParameterDescriptorList();
    private readonly int _hashCode; // precomputed
    private readonly Dictionary<string, ParameterDescriptor> _parameterNameIndex;
    private static readonly EqualityComparer<ParameterDescriptor> s_parameterEqualityComparer = EqualityComparer<ParameterDescriptor>.Create(
        (x, y) => x.ParameterPosition == y.ParameterPosition
            || x.ParameterName.Equals(y.ParameterName, StringComparison.Ordinal));

    public ParameterDescriptorList(ParameterDescriptor[] items) : this((IEnumerable<ParameterDescriptor>)items)
    {
    }

    public ParameterDescriptorList(ReadOnlySpan<ParameterDescriptor> items) : this(items.ToArray())
    {
    }

    public ParameterDescriptorList(IEnumerable<ParameterDescriptor> items)
    {
        Parameters = items?
            .OrderBy(parameter => parameter.ParameterPosition)
            .ToImmutableList()
            ?? ImmutableList<ParameterDescriptor>.Empty;
        _parameterNameIndex = Parameters.ToDictionary(parameter => parameter.ParameterName, StringComparer.Ordinal);
        ArgumentNullExceptionAdvanced.ThrowIfNull(Parameters, nameof(items));

        if (HasItems)
        {
            ValidateParametersOrThrow(nameof(items));
        }

        _hashCode = ComputeHashCode();
    }

    private void ValidateParametersOrThrow(string argumentName)
    {
        var parameterSet = new HashSet<ParameterDescriptor>(s_parameterEqualityComparer);
        foreach (ParameterDescriptor parameterDescriptor in Parameters)
        {
            if (!parameterSet.Add(parameterDescriptor))
            {
                throw new ArgumentException($"At least one item in the argument sequence '{argumentName}' has a duplicate value for the '{nameof(ParameterDescriptor)}.{nameof(ParameterDescriptor.ParameterPosition)}' parameter position or '{nameof(ParameterDescriptor)}.{nameof(ParameterDescriptor.ParameterName)}' parameter name.", argumentName);
            }

            if (parameterDescriptor.ParameterPosition > Parameters.Count)
            {
                throw new ArgumentException($"At least one item in the argument sequence '{argumentName}' has an invalid value for the '{nameof(ParameterDescriptor)}.{nameof(ParameterDescriptor.ParameterPosition)}' parameter position that exceeds the total count of parameters in the formal parameter list (expressed by the collection's item count).", argumentName);
            }
        }
    }

    internal ParameterDescriptorList(IEnumerable<ParameterDescriptor> items, bool isIntegrityValidationEnabled)
    {
        Parameters = items?
            .OrderBy(parameter => parameter.ParameterPosition)
            .ToImmutableList()
            ?? ImmutableList<ParameterDescriptor>.Empty;
        _parameterNameIndex = Parameters.ToDictionary(parameter => parameter.ParameterName, StringComparer.Ordinal);
        ArgumentNullExceptionAdvanced.ThrowIfNull(Parameters, nameof(items));

        if (HasItems)
        {
            ValidateParametersOrThrow(nameof(items));

        }

        _hashCode = ComputeHashCode();
    }

    private ParameterDescriptorList()
    {
        Parameters = ImmutableList<ParameterDescriptor>.Empty;
        _parameterNameIndex = new Dictionary<string, ParameterDescriptor>(0, StringComparer.Ordinal);
    }

    public bool TryGetParameterByName(string parameterName, out ParameterDescriptor parameterData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(parameterName);
        return _parameterNameIndex.TryGetValue(parameterName, out parameterData);
    }

    public bool ContainsParameterWithName(string parameterName)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(parameterName);
        return _parameterNameIndex.ContainsKey(parameterName);
    }

    public int Count => Parameters.Count;
    public bool IsEmpty => Parameters.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<ParameterDescriptor> Parameters { get; }

    public ParameterDescriptor this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Parameters.Count, nameof(index));

            return HasItems
                ? Parameters[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(ParameterDescriptorList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<ParameterDescriptor> GetEnumerator() => ((IEnumerable<ParameterDescriptor>)Parameters).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Parameters.GetEnumerator();

    public bool Equals(ParameterDescriptorList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!Parameters[index].Equals(other.Parameters[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is ParameterDescriptorList other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            for (int index = 0; index < Parameters.Count; index++)
            {
                hashCode.Add(Parameters[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(ParameterDescriptorList? left, ParameterDescriptorList? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(ParameterDescriptorList? left, ParameterDescriptorList? right) => !(left == right);
}
