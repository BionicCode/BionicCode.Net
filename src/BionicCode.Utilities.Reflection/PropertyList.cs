namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// Represents a read-only list of <see cref="PropertyData"/> items that belong to the same declaring type.
/// </summary>
internal sealed class PropertyList : IReadOnlyList<PropertyData>, IEquatable<PropertyList>
{
    public static PropertyList Empty { get; } = new PropertyList();
    private readonly int _hashCode; // precomputed
    private readonly TypeData? _declaringTypeData;
    private readonly Dictionary<string, PropertyData> _propertyNameIndex;

    public PropertyList(PropertyData[] items, TypeData? declaringType) : this((IEnumerable<PropertyData>)items, declaringType)
    {
    }

    public PropertyList(IEnumerable<PropertyData> items, TypeData? declaringType)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        _declaringTypeData = declaringType;

        Properties = items?.ToImmutableList() ?? ImmutableList<PropertyData>.Empty;
        _propertyNameIndex = Properties.ToDictionary(property => property.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Properties,
                property => !ReferenceEquals(property.DeclaringTypeData, _declaringTypeData),
                nameof(items),
                $"At least one item in the argument '{nameof(items)}' has a different value for the '{nameof(PropertyData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All properties must belong to the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    internal PropertyList(IEnumerable<PropertyData> items, TypeData? declaringType, bool isIntegrityValidationEnabled)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        _declaringTypeData = declaringType;

        Properties = items?.ToImmutableList() ?? ImmutableList<PropertyData>.Empty;
        _propertyNameIndex = Properties.ToDictionary(property => property.Name);

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Properties,
                property => !ReferenceEquals(property.DeclaringTypeData, _declaringTypeData),
                nameof(items),
                $"At least one item in the argument '{nameof(items)}' has a different value for the '{nameof(PropertyData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All properties must belong to the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    private PropertyList()
    {
        Properties = ImmutableList<PropertyData>.Empty;
        _propertyNameIndex = [];
        _declaringTypeData = default;
        _hashCode = ComputeHashCode();
    }

    public bool TryGetPropertyByName(string propertyName, out PropertyData? propertyData)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(propertyName);
        return _propertyNameIndex.TryGetValue(propertyName, out propertyData);
    }

    public int Count => Properties.Count;
    public bool IsEmpty => Properties.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<PropertyData> Properties { get; }
    public TypeData DeclaringTypeData => _declaringTypeData ?? throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(PropertyList), nameof(DeclaringTypeData)));

    public PropertyData this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Properties.Count, nameof(index));

            return HasItems
                ? Properties[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(PropertyList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<PropertyData> GetEnumerator() => ((IEnumerable<PropertyData>)Properties).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Properties.GetEnumerator();

    public bool Equals(PropertyList? other)
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
            if (!ReferenceEquals(Properties[index], other.Properties[index]))
            {
                return false;
            }
        }

        return true;
    }

    public bool Equals(IPropertyListView? other)
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
            if (!ReferenceEquals(Properties[index].View, other.Properties[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is PropertyList other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringTypeData);
            for (int index = 0; index < Properties.Count; index++)
            {
                hashCode.Add(Properties[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(PropertyList? left, PropertyList? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(PropertyList? left, PropertyList? right) => !(left == right);
    public static bool operator ==(PropertyList? left, IPropertyListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(PropertyList? left, IPropertyListView? right) => !(left == right);
    public static bool operator ==(IPropertyListView? left, PropertyList? right) => right == left;
    public static bool operator !=(IPropertyListView? left, PropertyList? right) => !(right == left);
}

/// <summary>
/// Represents a read-only list of <see cref="PropertyData"/> items that belong to the same declaring type.
/// </summary>
public sealed class PropertyListView : IReadOnlyList<IPropertyDataView>, IEquatable<IPropertyListView>, IPropertyListView
{
    public static IPropertyListView Empty { get; } = new PropertyListView();
    private readonly int _hashCode; // precomputed
    private readonly ITypeDataView? _declaringType;
    private readonly Dictionary<string, IPropertyDataView> _propertyNameIndex;

    public PropertyListView(IPropertyDataView[] items, ITypeDataView? declaringType) : this((IEnumerable<IPropertyDataView>)items, declaringType)
    {
    }

    public PropertyListView(IEnumerable<IPropertyDataView> items, ITypeDataView? declaringType)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        _declaringType = declaringType;
        Properties = items?.ToImmutableList() ?? ImmutableList<IPropertyDataView>.Empty;
        _propertyNameIndex = Properties.ToDictionary(property => property.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Properties,
                property => !ReferenceEquals(property.DeclaringType, _declaringType),
                nameof(items),
                $"At least one item in the argument '{nameof(items)}' has a different value for the '{nameof(IPropertyDataView)}.{nameof(IPropertyDataView.DeclaringType)}' declaring type handle. All properties must belong to the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    internal PropertyListView(IEnumerable<IPropertyDataView> items, ITypeDataView? declaringType, bool isIntegrityValidationEnabled)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        _declaringType = declaringType;
        Properties = items?.ToImmutableList() ?? ImmutableList<IPropertyDataView>.Empty;
        _propertyNameIndex = Properties.ToDictionary(property => property.Name);

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Properties,
                property => !ReferenceEquals(property.DeclaringType, _declaringType),
                nameof(items),
                $"At least one item in the argument '{nameof(items)}' has a different value for the '{nameof(IPropertyDataView)}.{nameof(IPropertyDataView.DeclaringType)}' declaring type handle. All properties must belong to the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    private PropertyListView()
    {
        Properties = ImmutableList<IPropertyDataView>.Empty;
        _propertyNameIndex = [];
        _declaringType = default;
        _hashCode = ComputeHashCode();
    }

    public bool TryGetPropertyByName(string propertyName, out IPropertyDataView? property)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(propertyName);
        return _propertyNameIndex.TryGetValue(propertyName, out property);
    }

    public bool ContainsPropertyWithName(string propertyName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(propertyName);
        return _propertyNameIndex.ContainsKey(propertyName);
    }

    public int Count => Properties.Count;
    public bool IsEmpty => Properties.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<IPropertyDataView> Properties { get; }
    public ITypeDataView DeclaringType => _declaringType ?? throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(IPropertyListView), nameof(DeclaringType)));

    public IPropertyDataView this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Properties.Count, nameof(index));

            return HasItems
                ? Properties[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(PropertyListView), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<IPropertyDataView> GetEnumerator() => ((IEnumerable<IPropertyDataView>)Properties).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Properties.GetEnumerator();

    public bool Equals(IPropertyListView? other)
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
            if (!ReferenceEquals(Properties[index], other.Properties[index]))
            {
                return false;
            }
        }

        return true;
    }

    internal bool Equals(PropertyList? other)
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

        for (int index = 0; index < Count; index++)
        {
            if (!ReferenceEquals(Properties[index], other.Properties[index].View))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is IPropertyListView other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringType);
            for (int index = 0; index < Properties.Count; index++)
            {
                hashCode.Add(Properties[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(PropertyListView? left, IPropertyListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(PropertyListView? left, IPropertyListView? right) => !(left == right);
    public static bool operator ==(IPropertyListView? left, PropertyListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(IPropertyListView? left, PropertyListView? right) => !(left == right);
}
