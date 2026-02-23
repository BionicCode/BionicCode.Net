namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// A read-only list of <see cref="MethodParameterInfo"/> items sorted by parameter position in ascending order.
/// </summary>
/// <remarks>The <see cref="MethodParameterInfo"/> items must belong to the same member of the same declaring type.
/// This collection is not intended for a loose collection of unrelated parameters.<br/>
/// Instead the collection is a strict representation of member parameters.</remarks>
internal sealed class MethodParameterInfoList : IReadOnlyList<MethodParameterInfo>, IEmptyCollectionProvider<MethodParameterInfoList>, IEquatable<MethodParameterInfoList>
{
    public static MethodParameterInfoList Empty { get; } = new MethodParameterInfoList();
    private readonly int _hashCode; // precomputed
    private readonly Dictionary<string, MethodParameterInfo> _parameterNameIndex;
    private static readonly EqualityComparer<MethodParameterInfo> s_parameterEqualityComparer = EqualityComparer<MethodParameterInfo>.Create(
        (x, y) => x.ParameterDescriptor.ParameterPosition == y.ParameterDescriptor.ParameterPosition
            || x.ParameterDescriptor.ParameterName.Equals(y.ParameterDescriptor.ParameterName, StringComparison.Ordinal));

    public MethodParameterInfoList(MethodParameterInfo[] items) : this((IEnumerable<MethodParameterInfo>)items)
    {
    }

    public MethodParameterInfoList(ReadOnlySpan<MethodParameterInfo> items) : this(items.ToArray())
    {
    }

    public MethodParameterInfoList(IEnumerable<MethodParameterInfo> items)
    {
        Parameters = items?
            .OrderBy(parameter => parameter.ParameterDescriptor.ParameterPosition)
            .ToImmutableList()
            ?? ImmutableList<MethodParameterInfo>.Empty;
        _parameterNameIndex = Parameters.ToDictionary(parameter => parameter.ParameterDescriptor.ParameterName, StringComparer.Ordinal);
        ArgumentNullExceptionAdvanced.ThrowIfNull(Parameters, nameof(items));

        if (HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfContainsDuplicate(
                Parameters,
                s_parameterEqualityComparer,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a duplicate value for the '{nameof(MethodParameterInfo.ParameterDescriptor)}.{nameof(AnonymousParameterDescriptor.ParameterPosition)}' parameter position or '{nameof(MethodParameterInfo.ParameterDescriptor)}.{nameof(AnonymousParameterDescriptor.ParameterName)}' parameter name.");
        }

        _hashCode = ComputeHashCode();
    }

    internal MethodParameterInfoList(IEnumerable<MethodParameterInfo> items, bool isIntegrityValidationEnabled)
    {
        Parameters = items?
            .OrderBy(parameter => parameter.ParameterDescriptor.ParameterPosition)
            .ToImmutableList()
            ?? ImmutableList<MethodParameterInfo>.Empty;
        _parameterNameIndex = Parameters.ToDictionary(parameter => parameter.ParameterDescriptor.ParameterName, StringComparer.Ordinal);
        ArgumentNullExceptionAdvanced.ThrowIfNull(Parameters, nameof(items));

        if (HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfContainsDuplicate(
                Parameters,
                s_parameterEqualityComparer,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a duplicate value for the '{nameof(MethodParameterInfo.ParameterDescriptor)}.{nameof(AnonymousParameterDescriptor.ParameterPosition)}' parameter position or '{nameof(MethodParameterInfo.ParameterDescriptor)}.{nameof(AnonymousParameterDescriptor.ParameterName)}' parameter name.");

        }

        _hashCode = ComputeHashCode();
    }

    private MethodParameterInfoList()
    {
        Parameters = ImmutableList<MethodParameterInfo>.Empty;
        _parameterNameIndex = new Dictionary<string, MethodParameterInfo>(0, StringComparer.Ordinal);
    }

    public bool TryGetParameterByName(string parameterName, out MethodParameterInfo parameterData)
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
    public ImmutableList<MethodParameterInfo> Parameters { get; }

    public MethodParameterInfo this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Parameters.Count, nameof(index));

            return HasItems
                ? Parameters[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodParameterInfoList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<MethodParameterInfo> GetEnumerator() => ((IEnumerable<MethodParameterInfo>)Parameters).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Parameters.GetEnumerator();

    public bool Equals(MethodParameterInfoList? other)
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

    public override bool Equals(object? obj) => obj is MethodParameterInfoList other && Equals(other);

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

    public static bool operator ==(MethodParameterInfoList? left, MethodParameterInfoList? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(MethodParameterInfoList? left, MethodParameterInfoList? right) => !(left == right);
}
