namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

[DebuggerDisplay($"Count = {{{nameof(Count)}}}")]
internal sealed class TypeList : IReadOnlyList<TypeData>, ICollection, IEmptyCollectionProvider<TypeList>, IEquatable<TypeList>
{
    public static TypeList Empty { get; } = new TypeList();
    private readonly int _hashCode; // precomputed
    private readonly ILookup<string, TypeData> _typeNameIndex;
    private ITypeListView? _view;

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
        typeList = _typeNameIndex[typeName].ToTypeList();
        return typeList.HasItems;
    }

    public bool ContainsTypeWithName(string typeName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(typeName);
        return _typeNameIndex.Contains(typeName);
    }

    public int Count => Types.Count;
    public bool IsEmpty => Types.IsEmpty;
    public bool HasItems => !IsEmpty;

    // Immutable collections are inherently thread-safe for read operations,
    // so we can consider this collection as synchronized for enumeration and access.
    public bool IsSynchronized { get; } = true;

    object ICollection.SyncRoot => this;
    public ImmutableList<TypeData> Types { get; }
    public ITypeListView View => _view ??= Types.ToTypeListView();

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

    public IEnumerator<TypeData> GetEnumerator() => ((IEnumerable<TypeData>)Types).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Types.GetEnumerator();

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
            if (!ReferenceEquals(Types[index], other.Types[index]))
            {
                return false;
            }
        }

        // Since the 'View' is generated internally from the Types collection of the TypeList
        // and at this point all items are reference equal,
        // then the 'View' instances are also reference equal.
        // No need to compare them separately.

        return true;
    }

    public override bool Equals(object? obj) => obj is TypeList other && Equals(other);

    public override int GetHashCode() => _hashCode;

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

    public void CopyTo(Array array, int index) => Types.CopyTo((TypeData[])array, index);

    public static bool operator ==(TypeList? left, TypeList? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(TypeList? left, TypeList? right) => !(left == right);
}

[DebuggerDisplay($"Count = {{{nameof(Count)}}}")]
public sealed class TypeListView : IReadOnlyList<ITypeDataView>, ICollection, IEmptyCollectionProvider<ITypeListView>, IEquatable<ITypeListView>, ITypeListView
{
    public static ITypeListView Empty { get; } = new TypeListView();
    private readonly int _hashCode; // precomputed
    private readonly ILookup<string, ITypeDataView> _typeNameIndex;

    public TypeListView(ITypeDataView[] items) : this((IEnumerable<ITypeDataView>)items)
    {
    }

    public TypeListView(IEnumerable<ITypeDataView> items)
    {
        Types = items?.ToImmutableList() ?? ImmutableList<ITypeDataView>.Empty;
        _typeNameIndex = Types.ToLookup(type => type.Name, StringComparer.Ordinal);
        _hashCode = ComputeHashCode();
    }

    internal TypeListView(IEnumerable<TypeData> items)
    {
        Types = items.Select(item => item.View)?.ToImmutableList() ?? ImmutableList<ITypeDataView>.Empty;
        _typeNameIndex = Types.ToLookup(type => type.Name, StringComparer.Ordinal);
        _hashCode = ComputeHashCode();
    }

    private TypeListView()
    {
        Types = ImmutableList<ITypeDataView>.Empty;
        _typeNameIndex = Types.ToLookup(type => type.Name, StringComparer.Ordinal);
    }

    public bool TryGetTypesByName(string typeName, out ITypeListView typeList)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(typeName);
        typeList = _typeNameIndex[typeName].ToTypeListView();
        return typeList.HasItems;
    }

    public bool ContainsTypeWithName(string typeName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(typeName);
        return _typeNameIndex.Contains(typeName);
    }

    public int Count => Types.Count;
    public bool IsEmpty => Types.IsEmpty;
    public bool HasItems => !IsEmpty;

    // Immutable collections are inherently thread-safe for read operations,
    // so we can consider this collection as synchronized for enumeration and access.
    public bool IsSynchronized { get; } = true;

    object ICollection.SyncRoot => this;
    public ImmutableList<ITypeDataView> Types { get; }

    public ITypeDataView this[int index]
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

    public IEnumerator<ITypeDataView> GetEnumerator() => ((IEnumerable<ITypeDataView>)Types).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Types.GetEnumerator();

    public bool Equals(ITypeListView? other)
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
            if (!ReferenceEquals(Types[index], other.Types[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is ITypeListView other && Equals(other);

    public override int GetHashCode() => _hashCode;

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

    public void CopyTo(Array array, int index) => Types.CopyTo((ITypeDataView[])array, index);

    public static bool operator ==(TypeListView? left, ITypeListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(TypeListView? left, ITypeListView? right) => !(left == right);
}
