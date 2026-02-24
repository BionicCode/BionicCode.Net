namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

[DebuggerDisplay($"Count = {{{nameof(Count)}}}")]
internal sealed class ConstructorList : IReadOnlyList<ConstructorData>, ICollection, IEmptyCollectionProvider<ConstructorList>, IEquatable<ConstructorList>
{
    public static ConstructorList Empty { get; } = new ConstructorList();
    private readonly int _hashCode; // precomputed
    private readonly TypeData _declaringTypeData;
    private IConstructorListView? _view;

    public ConstructorList(ConstructorData[] items, TypeData? declaringType) : this((IEnumerable<ConstructorData>)items, declaringType)
    {
    }

    public ConstructorList(IEnumerable<ConstructorData> items, TypeData? declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        _declaringTypeData = declaringType;

        Constructors = items?.ToImmutableList() ?? ImmutableList<ConstructorData>.Empty;
        if (HasItems)
        {

            ArgumentExceptionAdvanced.ThrowIfAny(
                Constructors,
                constructorData => !ReferenceEquals(constructorData.DeclaringTypeData, _declaringTypeData),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(ConstructorData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All constructors must belong to the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    internal ConstructorList(IEnumerable<ConstructorData> items, TypeData? declaringType, bool isIntegrityValidationEnabled)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        _declaringTypeData = declaringType;

        Constructors = items?.ToImmutableList() ?? ImmutableList<ConstructorData>.Empty;
        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Constructors,
                constructorData => !ReferenceEquals(constructorData.DeclaringTypeData, _declaringTypeData),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(ConstructorData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All constructors must belong to the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    private ConstructorList()
    {
        Constructors = ImmutableList<ConstructorData>.Empty;
        _declaringTypeData = default!;
        _hashCode = ComputeHashCode();
    }

    public int Count => Constructors.Count;
    public bool IsEmpty => Constructors.IsEmpty;
    public bool HasItems => !IsEmpty;

    // Immutable collections are inherently thread-safe for read operations,
    // so we can consider this collection as synchronized for enumeration and access.
    public bool IsSynchronized { get; } = true;

    object ICollection.SyncRoot => this;
    public ImmutableList<ConstructorData> Constructors { get; }
    public IConstructorListView View => _view ??= Constructors.ToConstructorListView(DeclaringTypeData);
    public TypeData DeclaringTypeData => _declaringTypeData ?? throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringTypeData)));

    public ConstructorData this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Constructors.Count, nameof(index));

            return HasItems
                ? Constructors[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(ConstructorList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<ConstructorData> GetEnumerator() => ((IEnumerable<ConstructorData>)Constructors).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Constructors.GetEnumerator();

    public bool Equals(ConstructorList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringTypeData, other.DeclaringTypeData))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!ReferenceEquals(Constructors[index], other.Constructors[index]))
            {
                return false;
            }
        }

        return true;
    }

    public bool Equals(IConstructorListView? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringTypeData.View, other.DeclaringType))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!Constructors[index].View.Equals(other.Constructors[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is ConstructorList other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringTypeData);
            for (int index = 0; index < Constructors.Count; index++)
            {
                hashCode.Add(Constructors[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public void CopyTo(Array array, int index) => Constructors.CopyTo((ConstructorData[])array, index);

    public static bool operator ==(ConstructorList? left, ConstructorList? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(ConstructorList? left, ConstructorList? right) => !(left == right);

    public static bool operator ==(IConstructorListView? left, ConstructorList? right) => right == left;
    public static bool operator !=(IConstructorListView? left, ConstructorList? right) => !(left == right);

    public static bool operator ==(ConstructorList? left, IConstructorListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(ConstructorList? left, IConstructorListView? right) => !(left == right);
}

[DebuggerDisplay($"Count = {{{nameof(Count)}}}")]
public sealed class ConstructorListView : IReadOnlyList<IConstructorDataView>, ICollection, IEmptyCollectionProvider<IConstructorListView>, IEquatable<IConstructorListView>, IConstructorListView
{
    public static IConstructorListView Empty { get; } = new ConstructorListView();
    private readonly int _hashCode; // precomputed
    private readonly ITypeDataView _declaringType;
    public ConstructorListView(IConstructorDataView[] items, ITypeDataView? declaringType) : this((IEnumerable<IConstructorDataView>)items, declaringType)
    {
    }

    public ConstructorListView(IEnumerable<IConstructorDataView> items, ITypeDataView? declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        _declaringType = declaringType;
        Constructors = items?.ToImmutableList() ?? ImmutableList<IConstructorDataView>.Empty;
        if (HasItems)
        {

            ArgumentExceptionAdvanced.ThrowIfAny(
                Constructors,
                constructorData => !ReferenceEquals(constructorData.DeclaringType, _declaringType),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(IConstructorDataView.DeclaringType)}' declaring type handle. All constructors must belong to the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    internal ConstructorListView(IEnumerable<IConstructorDataView> items, ITypeDataView? declaringType, bool isIntegrityValidationEnabled)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        _declaringType = declaringType;
        Constructors = items?.ToImmutableList() ?? ImmutableList<IConstructorDataView>.Empty;
        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Constructors,
                constructorData => !ReferenceEquals(constructorData.DeclaringType, _declaringType),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(IConstructorDataView.DeclaringType)}' declaring type handle. All constructors must belong to the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    private ConstructorListView()
    {
        Constructors = ImmutableList<IConstructorDataView>.Empty;
        _declaringType = default!;
        _hashCode = ComputeHashCode();
    }

    public int Count => Constructors.Count;
    public bool IsEmpty => Constructors.IsEmpty;
    public bool HasItems => !IsEmpty;

    // Immutable collections are inherently thread-safe for read operations,
    // so we can consider this collection as synchronized for enumeration and access.
    public bool IsSynchronized { get; } = true;

    object ICollection.SyncRoot => this;
    public ImmutableList<IConstructorDataView> Constructors { get; }
    public ITypeDataView DeclaringType => _declaringType ?? throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringType)));

    public IConstructorDataView this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Constructors.Count, nameof(index));

            return HasItems
                ? Constructors[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(ConstructorListView), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<IConstructorDataView> GetEnumerator() => ((IEnumerable<IConstructorDataView>)Constructors).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Constructors.GetEnumerator();

    public bool Equals(IConstructorListView? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringType, other.DeclaringType))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!Constructors[index].Equals(other.Constructors[index]))
            {
                return false;
            }
        }

        return true;
    }

    internal static bool Equals(IConstructorListView? constructorDataViews, ConstructorList? constructorList)
    {
        // Return FALSE if exactly one of the constructor lists is NULL, otherwise compare the constructor lists for equality.
        if (constructorDataViews is null ^ constructorList is null)
        {
            return false;
        }

        // If the constructor list view is NULL, the constructor list must also be NULL at this point, so return TRUE.
        // If both constructor lists are NULL, consider them equal.
        if (constructorDataViews is null)
        {
            return true;
        }

        if (constructorDataViews.Count != constructorList!.Count)
        {
            return false;
        }

        if (!ReferenceEquals(constructorDataViews.DeclaringType, constructorList.DeclaringTypeData.View))
        {
            return false;
        }

        for (int index = 0; index < constructorDataViews.Count; index++)
        {
            if (!ReferenceEquals(constructorDataViews[index], constructorList.Constructors[index].View))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is ConstructorListView other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringType);
            for (int index = 0; index < Constructors.Count; index++)
            {
                hashCode.Add(Constructors[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public void CopyTo(Array array, int index) => Constructors.CopyTo((IConstructorDataView[])array, index);

    public static bool operator ==(ConstructorListView? left, IConstructorListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(ConstructorListView? left, IConstructorListView? right) => !(left == right);

    public static bool operator ==(IConstructorListView? left, ConstructorListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(IConstructorListView? left, ConstructorListView? right) => !(left == right);
}
