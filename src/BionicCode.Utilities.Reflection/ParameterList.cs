namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

/// <summary>
/// A read-only list of <see cref="ParameterData"/> items sorted by parameter position in ascending order.
/// </summary>
/// <remarks>The <see cref="ParameterData"/> items must belong to the same member of the same declaring type.
/// This collection is not intended for a loose collection of unrelated parameters.<br/>
/// Instead the collection is a strict representation of member parameters.</remarks>
internal sealed class ParameterList : IReadOnlyList<ParameterData>, IEquatable<ParameterList>
{
    public static ParameterList Empty { get; } = new ParameterList();
    private readonly int _hashCode; // precomputed
    private readonly SymbolReflectionInfoCacheKeyInternal _declaringMemberCacheKey;
    private readonly Dictionary<string, ParameterData> _parameterNameIndex;

    private ParameterList()
    {
        Parameters = ImmutableList<ParameterData>.Empty;
        _parameterNameIndex = new Dictionary<string, ParameterData>(0, StringComparer.Ordinal);
    }

    public ParameterList(ParameterData[] items) : this((IEnumerable<ParameterData>)items)
    {
    }

    public ParameterList(IEnumerable<ParameterData> items)
    {
        Parameters = items?.OrderBy(parameter => parameter.Position).ToImmutableList()
            ?? ImmutableList<ParameterData>.Empty;
        _parameterNameIndex = Parameters.ToDictionary(parameter => parameter.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            ParameterizedMemberData declaringMember = Parameters.First().MemberData;
            ArgumentNullExceptionAdvanced.ThrowIfNull(
                declaringMember,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has no value for the '{nameof(ParameterData)}.{nameof(ParameterData.MemberData)}' declaring member. All parameters must belong to the same member of the same declaring type.");

            _declaringMemberCacheKey = declaringMember.CacheKey;
            RuntimeMethodHandle declaringMemberHandle = declaringMember.Handle;
            RuntimeTypeHandle declaringTypeHandle = declaringMember.DeclaringTypeHandle;

            ArgumentExceptionAdvanced.ThrowIfAny(
                Parameters,
                parameterData => parameterData.MemberData.Handle != declaringMemberHandle || !parameterData.MemberData.DeclaringTypeHandle.Equals(declaringTypeHandle),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(ParameterData)}.{nameof(ParameterData.MemberData)}' declaring member handle. All parameters must belong to the same member of the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    internal ParameterList(IEnumerable<ParameterData> items, bool isIntegrityValidationEnabled)
    {
        Parameters = items?.OrderBy(parameter => parameter.Position)
            .ToImmutableList()
            ?? ImmutableList<ParameterData>.Empty;
        _parameterNameIndex = Parameters.ToDictionary(parameter => parameter.Name, StringComparer.Ordinal);

        ParameterizedMemberData? declaringMember = null;
        if (HasItems)
        {
            declaringMember = Parameters.First().MemberData;
            ArgumentNullExceptionAdvanced.ThrowIfNull(
                declaringMember,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has no value for the '{nameof(ParameterData)}.{nameof(ParameterData.MemberData)}' declaring member. All parameters must belong to the same member of the same declaring type.");

            _declaringMemberCacheKey = declaringMember.CacheKey;
        }

        if (isIntegrityValidationEnabled && HasItems)
        {
            RuntimeMethodHandle declaringMemberHandle = declaringMember!.Handle;
            RuntimeTypeHandle declaringTypeHandle = declaringMember!.DeclaringTypeHandle;

            ArgumentExceptionAdvanced.ThrowIfAny(
                Parameters,
                parameterData => parameterData.MemberData.Handle != declaringMemberHandle || !parameterData.MemberData.DeclaringTypeHandle.Equals(declaringTypeHandle),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(ParameterData)}.{nameof(ParameterData.MemberData)}' declaring member handle. All parameters must belong to the same member of the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    public ImmutableList<ParameterInfo> AsParameterInfoList()
        => Parameters
            .Select(parameterData => parameterData.ParameterInfo())
            .ToImmutableList();

    public ImmutableArray<ParameterInfo> AsParameterInfoArray()
        => Parameters
            .Select(parameterData => parameterData.ParameterInfo())
            .ToImmutableArray();

    public bool TryGetParameterByName(string parameterName, out ParameterData? parameterData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(parameterName);
        return _parameterNameIndex.TryGetValue(parameterName, out parameterData);
    }

    public int Count => Parameters.Count;
    public bool IsEmpty => Parameters.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<ParameterData> Parameters { get; }
    public SymbolReflectionInfoCacheKeyInternal DeclaringMemberCacheKey
        => HasItems
            ? _declaringMemberCacheKey
            : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringMemberCacheKey)));

    public ParameterizedMemberData DeclaringMemberData
    {
        get
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringMemberData)));
            }

            SymbolReflectionInfoCacheKeyInternal cacheKey = DeclaringMemberCacheKey;
            return cacheKey.SymbolKind == SymbolKind.MemberMethod
                    ? SymbolReflectionInfoCache.GetOrCreateMethodDataCacheEntry(ref cacheKey)
                    : SymbolReflectionInfoCache.GetOrCreateConstructorDataCacheEntry(ref cacheKey);
        }
    }

    public ParameterData this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Parameters.Count, nameof(index));

            return HasItems
                ? Parameters[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(ParameterList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<ParameterData> GetEnumerator()
        => ((IEnumerable<ParameterData>)Parameters).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => Parameters.GetEnumerator();

    public bool Equals(ParameterList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!DeclaringMemberCacheKey.Equals(other.DeclaringMemberCacheKey))
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

    public override bool Equals(object? obj)
        => obj is ParameterList other && Equals(other);

    public override int GetHashCode()
        => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringMemberCacheKey);
            for (int index = 0; index < Parameters.Count; index++)
            {
                hashCode.Add(Parameters[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(ParameterList? left, ParameterList? right)
        => left?.Equals(right) ?? (right is null);
    public static bool operator !=(ParameterList? left, ParameterList? right)
        => !(left == right);
}
