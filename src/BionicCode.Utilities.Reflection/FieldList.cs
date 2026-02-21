namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

internal sealed class FieldList : IReadOnlyList<FieldData>, IEquatable<FieldList>
{
    public static FieldList Empty { get; } = new FieldList();
    private readonly int _hashCode; // precomputed
    private readonly Dictionary<string, FieldData> _fieldNameIndex;
    private readonly TypeData? _declaringTypeData;

    public FieldList(FieldData[] items, TypeData? declaringType) : this((IEnumerable<FieldData>)items, declaringType)
    {
    }

    public FieldList(IEnumerable<FieldData> items, TypeData? declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);
        _declaringTypeData = declaringType;

        Fields = items?.ToImmutableList() ?? ImmutableList<FieldData>.Empty;
        _fieldNameIndex = Fields.ToDictionary(fieldData => fieldData.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Fields,
                fieldData => !ReferenceEquals(fieldData.DeclaringTypeData, _declaringTypeData),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(FieldData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All fields must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    internal FieldList(IEnumerable<FieldData> items, TypeData? declaringType, bool isIntegrityValidationEnabled)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);
        _declaringTypeData = declaringType;

        Fields = items?.ToImmutableList() ?? ImmutableList<FieldData>.Empty;
        _fieldNameIndex = Fields.ToDictionary(fieldData => fieldData.Name, StringComparer.Ordinal);

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Fields,
                fieldData => !ReferenceEquals(fieldData.DeclaringTypeData, _declaringTypeData),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(FieldData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All fields must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    private FieldList()
    {
        Fields = ImmutableList<FieldData>.Empty;
        _fieldNameIndex = [];
        _declaringTypeData = null;
        _hashCode = ComputeHashCode();
    }

    public bool TryGetFieldByName(string fieldName, out FieldData? fieldData)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(fieldName);
        return _fieldNameIndex.TryGetValue(fieldName, out fieldData!);
    }

    public bool ContainsFieldWithName(string fieldName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(fieldName);
        return _fieldNameIndex.ContainsKey(fieldName);
    }

    public int Count => Fields.Count;
    public bool IsEmpty => Fields.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<FieldData> Fields { get; }
    public TypeData DeclaringTypeData => _declaringTypeData ?? throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(FieldList), nameof(DeclaringTypeData)));

    public FieldData this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Fields.Count, nameof(index));

            return HasItems
                ? Fields[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(FieldList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<FieldData> GetEnumerator() => ((IEnumerable<FieldData>)Fields).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Fields.GetEnumerator();

    public bool Equals(FieldList? other)
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

        bool isEqual = false;
        for (int index = 0; index < Count && !isEqual; index++)
        {
            if (!ReferenceEquals(Fields[index], other.Fields[index]))
            {
                return false;
            }
        }

        return true;
    }

    public bool Equals(IFieldListView? other)
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

        bool isEqual = false;
        for (int index = 0; index < Count && !isEqual; index++)
        {
            if (!ReferenceEquals(Fields[index].View, other.Fields[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is FieldList other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringTypeData);
            for (int index = 0; index < Fields.Count; index++)
            {
                hashCode.Add(Fields[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(FieldList? left, FieldList? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(FieldList? left, FieldList? right) => !(left == right);
    public static bool operator ==(FieldList? left, IFieldListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(FieldList? left, IFieldListView? right) => !(left == right);
    public static bool operator ==(IFieldListView? left, FieldList? right) => right == left;
    public static bool operator !=(IFieldListView? left, FieldList? right) => !(right == left);
}

public sealed class FieldListView : IReadOnlyList<IFieldDataView>, IEquatable<IFieldListView>, IFieldListView
{
    public static IFieldListView Empty { get; } = new FieldListView();
    private readonly int _hashCode; // precomputed
    private readonly Dictionary<string, IFieldDataView> _fieldNameIndex;
    private readonly ITypeDataView? _declaringType;

    public FieldListView(IFieldDataView[] items, ITypeDataView? declaringType) : this((IEnumerable<IFieldDataView>)items, declaringType)
    {
    }

    public FieldListView(IEnumerable<IFieldDataView> items, ITypeDataView? declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);
        _declaringType = declaringType;

        Fields = items?.ToImmutableList() ?? ImmutableList<IFieldDataView>.Empty;
        _fieldNameIndex = Fields.ToDictionary(fieldData => fieldData.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Fields,
                fieldData => !ReferenceEquals(fieldData.DeclaringType, _declaringType),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(IFieldDataView.DeclaringType)}' declaring type handle. All fields must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    internal FieldListView(IEnumerable<IFieldDataView> items, ITypeDataView? declaringType, bool isIntegrityValidationEnabled)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);
        _declaringType = declaringType;

        Fields = items?.ToImmutableList() ?? ImmutableList<IFieldDataView>.Empty;
        _fieldNameIndex = Fields.ToDictionary(fieldData => fieldData.Name, StringComparer.Ordinal);

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Fields,
                fieldData => !ReferenceEquals(fieldData.DeclaringType, _declaringType),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(IFieldDataView.DeclaringType)}' declaring type handle. All fields must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    private FieldListView()
    {
        Fields = ImmutableList<IFieldDataView>.Empty;
        _fieldNameIndex = [];
        _declaringType = null;
        _hashCode = ComputeHashCode();
    }

    public bool TryGetFieldByName(string fieldName, out IFieldDataView? fieldData)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(fieldName);
        return _fieldNameIndex.TryGetValue(fieldName, out fieldData!);
    }

    public bool ContainsFieldWithName(string fieldName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(fieldName);
        return _fieldNameIndex.ContainsKey(fieldName);
    }

    public int Count => Fields.Count;
    public bool IsEmpty => Fields.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<IFieldDataView> Fields { get; }
    public ITypeDataView DeclaringType => _declaringType ?? throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(FieldList), nameof(DeclaringType)));

    public IFieldDataView this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Fields.Count, nameof(index));

            return HasItems
                ? Fields[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(FieldList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<IFieldDataView> GetEnumerator() => ((IEnumerable<IFieldDataView>)Fields).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Fields.GetEnumerator();

    public bool Equals(IFieldListView? other)
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

        bool isEqual = false;
        for (int index = 0; index < Count && !isEqual; index++)
        {
            if (!ReferenceEquals(Fields[index], other.Fields[index]))
            {
                return false;
            }
        }

        return true;
    }

    internal bool Equals(FieldList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringType, other.DeclaringTypeData.View))
        {
            return false;
        }

        bool isEqual = false;
        for (int index = 0; index < Count && !isEqual; index++)
        {
            if (!ReferenceEquals(Fields[index], other.Fields[index].View))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is IFieldListView other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringType);
            for (int index = 0; index < Fields.Count; index++)
            {
                hashCode.Add(Fields[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(FieldListView? left, IFieldListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(FieldListView? left, IFieldListView? right) => !(left == right);
    public static bool operator ==(IFieldListView? left, FieldListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(IFieldListView? left, FieldListView? right) => !(left == right);
}
