namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

internal sealed class TypeList : IReadOnlyList<TypeData>, IEquatable<TypeList>
{
    public static TypeList Empty { get; } = new TypeList();
    private readonly int _hashCode; // precomputed
    private readonly ILookup<string, TypeData> _typeNameIndex;

    public TypeList(TypeData[] items) : this((IEnumerable<TypeData>)items)
    {
    }

    public TypeList(IEnumerable<TypeData> items)
    {
        Types = items?.ToImmutableList() ?? ImmutableList<TypeData>.Empty;
        _typeNameIndex = Types.ToLookup(type => type.Name, StringComparer.Ordinal);
        _hashCode = ComputeHashCode();
    }

    private TypeList()
    {
        Types = ImmutableList<TypeData>.Empty;
        _typeNameIndex = Types.ToLookup(type => type.Name, StringComparer.Ordinal);
    }

    public bool TryGetTypesByName(string typeName, out TypeList typeList)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(typeName);
        typeList = _typeNameIndex[typeName]
            .ToTypeList();
        return typeList.HasItems;
    }

    public int Count => Types.Count;
    public bool IsEmpty => Types.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<TypeData> Types { get; }

    public TypeData this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Types.Count, nameof(index));

            return HasItems
                ? Types[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(TypeList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<TypeData> GetEnumerator()
        => ((IEnumerable<TypeData>)Types).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => Types.GetEnumerator();

    public bool Equals(TypeList? other)
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
            if (!Types[index].Equals(other.Types[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is TypeList other && Equals(other);

    public override int GetHashCode()
        => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            for (int index = 0; index < Types.Count; index++)
            {
                hashCode.Add(Types[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(TypeList? left, TypeList? right)
        => left?.Equals(right) ?? (right is null);
    public static bool operator !=(TypeList? left, TypeList? right)
        => !(left == right);
}
