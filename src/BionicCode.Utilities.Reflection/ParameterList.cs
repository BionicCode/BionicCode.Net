namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
#region ParameterList

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
    private readonly ParameterizedMemberData? _declaringMember;
    private readonly Dictionary<string, ParameterData> _parameterNameIndex;

    private ParameterList()
    {
        _declaringMember = null;
        Parameters = ImmutableList<ParameterData>.Empty;
        _parameterNameIndex = [];

        _hashCode = ComputeHashCode();
    }

    public ParameterList(ParameterData[] items, ParameterizedMemberData declaringMember) : this((IEnumerable<ParameterData>)items, declaringMember)
    {
    }

    public ParameterList(IEnumerable<ParameterData> items, ParameterizedMemberData declaringMember)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(
            declaringMember,
            nameof(declaringMember),
            $"The argument '{nameof(declaringMember)}' cannot be null. All parameters must belong to the same valid member of the same declaring type.");

        _declaringMember = declaringMember;

        Parameters = items?
            .OrderBy(parameter => parameter.Position)
            .ToImmutableList()
            ?? ImmutableList<ParameterData>.Empty;
        _parameterNameIndex = Parameters.ToDictionary(parameter => parameter.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Parameters,
                parameterData => ReferenceEquals(parameterData.MemberData, _declaringMember),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(ParameterData)}.{nameof(ParameterData.MemberData)}' declaring member handle. All parameters must belong to the same member of the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    internal ParameterList(IEnumerable<ParameterData> items, ParameterizedMemberData declaringMember, bool isIntegrityValidationEnabled)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(
            declaringMember,
            nameof(declaringMember),
            $"The argument '{nameof(declaringMember)}' cannot be null. All parameters must belong to the same valid member of the same declaring type.");

        _declaringMember = declaringMember;

        Parameters = items?
            .OrderBy(parameter => parameter.Position)
            .ToImmutableList()
            ?? ImmutableList<ParameterData>.Empty;
        _parameterNameIndex = Parameters.ToDictionary(parameter => parameter.Name, StringComparer.Ordinal);

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Parameters,
                parameterData => ReferenceEquals(parameterData.MemberData, _declaringMember),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(ParameterData)}.{nameof(ParameterData.MemberData)}' declaring member handle. All parameters must belong to the same member of the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    public ImmutableArray<ParameterInfo> AsParameterInfoArray()
        => Parameters
            .Select(parameterData => parameterData.ParameterInfo)
            .ToImmutableArray();

    public bool TryGetParameterByName(string parameterName, out ParameterData? parameterData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(parameterName);
        return _parameterNameIndex.TryGetValue(parameterName, out parameterData);
    }

    public bool ContainsTypeWithName(string typeName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(typeName);
        return _parameterNameIndex.ContainsKey(typeName);
    }

    public int Count => Parameters.Count;
    public bool IsEmpty => Parameters.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<ParameterData> Parameters { get; }

    public ParameterizedMemberData DeclaringMemberData => _declaringMember ?? throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringMemberData)));

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

    public IEnumerator<ParameterData> GetEnumerator() => ((IEnumerable<ParameterData>)Parameters).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Parameters.GetEnumerator();

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

        if (!ReferenceEquals(DeclaringMemberData, other.DeclaringMemberData))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!ReferenceEquals(Parameters[index], other.Parameters[index]))
            {
                return false;
            }
        }

        return true;
    }

    public bool Equals(IParameterListView? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringMemberData.View, other.DeclaringMember))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!Parameters[index].View.Equals(other.Parameters[index]))
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
            hashCode.Add(DeclaringMemberData);
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
    public static bool operator ==(ParameterList? left, IParameterListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(ParameterList? left, IParameterListView? right) => !(left == right);

    public static bool operator ==(IParameterListView? left, ParameterList? right) => right == left;
    public static bool operator !=(IParameterListView? left, ParameterList? right) => !(left == right);
}
#endregion ParameterList

public sealed class ParameterListView : IReadOnlyList<IParameterDataView>, IEquatable<IParameterListView>, IParameterListView
{
    public static IParameterListView Empty { get; } = new ParameterListView();
    private readonly int _hashCode; // precomputed
    private readonly IParameterizedMemberDataView? _declaringMemberView;
    private readonly Dictionary<string, IParameterDataView> _parameterNameIndex;

    private ParameterListView()
    {
        _declaringMemberView = null;
        Parameters = ImmutableList<IParameterDataView>.Empty;
        _parameterNameIndex = [];

        _hashCode = ComputeHashCode();
    }

    public ParameterListView(IParameterDataView[] items, IParameterizedMemberDataView declaringMember) : this((IEnumerable<IParameterDataView>)items, declaringMember)
    {
    }

    public ParameterListView(IEnumerable<IParameterDataView> items, IParameterizedMemberDataView declaringMember)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(
            declaringMember,
            nameof(declaringMember),
            $"The argument '{nameof(declaringMember)}' cannot be null. All parameters must belong to the same valid member of the same declaring type.");

        _declaringMemberView = declaringMember;

        Parameters = items?.OrderBy(parameter => parameter.Position).ToImmutableList()
            ?? ImmutableList<IParameterDataView>.Empty;
        _parameterNameIndex = Parameters.ToDictionary(parameter => parameter.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            RuntimeMethodHandle declaringMemberHandle = _declaringMemberView.Handle;
            RuntimeTypeHandle declaringTypeHandle = _declaringMemberView.DeclaringTypeHandle;

            ArgumentExceptionAdvanced.ThrowIfAny(
                Parameters,
                parameterData => parameterData.MemberData.Handle != declaringMemberHandle || !parameterData.MemberData.DeclaringTypeHandle.Equals(declaringTypeHandle),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(IParameterDataView)}.{nameof(IParameterDataView.MemberData)}' declaring member handle. All parameters must belong to the same member of the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    internal ParameterListView(IEnumerable<IParameterDataView> items, IParameterizedMemberDataView declaringMember, bool isIntegrityValidationEnabled)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(
            declaringMember,
            nameof(declaringMember),
            $"The argument '{nameof(declaringMember)}' cannot be null. All parameters must belong to the same valid member of the same declaring type.");

        _declaringMemberView = declaringMember;

        Parameters = items?.OrderBy(parameter => parameter.Position)
            .ToImmutableList()
            ?? ImmutableList<IParameterDataView>.Empty;
        _parameterNameIndex = Parameters.ToDictionary(parameter => parameter.Name, StringComparer.Ordinal);

        if (isIntegrityValidationEnabled && HasItems)
        {
            RuntimeMethodHandle declaringMemberHandle = _declaringMemberView.Handle;
            RuntimeTypeHandle declaringTypeHandle = _declaringMemberView.DeclaringTypeHandle;

            ArgumentExceptionAdvanced.ThrowIfAny(
                Parameters,
                parameterData => parameterData.MemberData.Handle != declaringMemberHandle || !parameterData.MemberData.DeclaringTypeHandle.Equals(declaringTypeHandle),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(IParameterDataView)}.{nameof(IParameterDataView.MemberData)}' declaring member handle. All parameters must belong to the same member of the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    public bool TryGetParameterByName(string parameterName, out IParameterDataView? parameterData)
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
    public ImmutableList<IParameterDataView> Parameters { get; }
    public IParameterizedMemberDataView DeclaringMember => _declaringMemberView ?? throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringMember)));

    public IParameterDataView this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Parameters.Count, nameof(index));

            return HasItems
                ? Parameters[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(ParameterListView), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<IParameterDataView> GetEnumerator()
        => ((IEnumerable<IParameterDataView>)Parameters).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => Parameters.GetEnumerator();

    public bool Equals(IParameterListView? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringMember, other.DeclaringMember))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!ReferenceEquals(Parameters[index], other.Parameters[index]))
            {
                return false;
            }
        }

        return true;
    }

    internal bool Equals(ParameterList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringMember, other.DeclaringMemberData.View))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!Parameters[index].Equals(other.Parameters[index].View))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is IParameterListView other && Equals(other);

    public override int GetHashCode()
        => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringMember.Handle);
            for (int index = 0; index < Parameters.Count; index++)
            {
                hashCode.Add(Parameters[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(IParameterListView? left, ParameterListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(IParameterListView? left, ParameterListView? right) => !(left == right);
    public static bool operator ==(ParameterListView? left, IParameterListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(ParameterListView? left, IParameterListView? right) => !(left == right);
}
