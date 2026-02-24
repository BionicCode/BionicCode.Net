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
/// <item>have unique parameter positions or unique parameter names to avoid ambiguity when used for lookups. Duplicate positions or names are not allowed and will result in an exception.</item>
/// <item>belong to the same member of the same declaring type.</item>
/// <item>have the same declaring method parameter count.</item>
/// <item>the total count of <see cref="ParameterDescriptor"/> items does not exceed the declaring method parameter count.</item>
/// <item>the parameter list is complete or incomplete. When incomplete then the parameter position and the declaring method's parameter count must be specified; otherwise lookup operations may be ambiguous and may or m ay not silently fail.</item>
/// </list>
/// <para/>Ambiguity is defined as the lack of sufficient information that leads to multiple method candidates during the lookup. 
/// <para/>To avoid ambiguity and to significantly improve the accuracy and performance of lookups, it is highly recommended to construct the <see cref="ParameterDescriptor"/> instances using all available information.
/// <para/>See the <see cref="ParameterDescriptor"/> documentation for more information on how to construct the descriptors and the expected values for their properties to ensure maximum accuracy and performance when used for lookups.
/// <para/>If <see cref="ParameterDescriptor.ParameterPosition"/> is provided then the collection is sorted by parameter position in ascending order.
/// <para/>To ensure the integrity of the collection, the constructor validates the provided items for duplicates and consistency of declaring method parameter count when applicable. If any validation fails, an appropriate exception is thrown to indicate the specific issue with the input collection.
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
    private bool? _isSortedByParameterPosition;

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
            DeclaringMethodParameterCount = ValidateParameters(nameof(items));
        }

        _hashCode = ComputeHashCode();
    }

    private int ValidateParameters(string argumentName)
    {
        ArgumentExceptionAdvanced.ThrowIfContainsDuplicate(
            Parameters,
            s_parameterEqualityComparer,
            argumentName,
            $"At least one item in the argument sequence '{argumentName}' has a duplicate value for the '{nameof(ParameterDescriptor)}.{nameof(ParameterDescriptor.ParameterPosition)}' parameter position or '{nameof(ParameterDescriptor)}.{nameof(ParameterDescriptor.ParameterName)}' parameter name.");

        int declaringMemberParameterCount = ParameterDescriptor.UnknownParameterCountOrPosition;
        for (int index = 0; index < Parameters.Count; index++)
        {
            ParameterDescriptor parameter = Parameters[index];
            if (parameter.HasDeclaringMethodParameterCount)
            {
                if (declaringMemberParameterCount == ParameterDescriptor.UnknownParameterCountOrPosition)
                {
                    declaringMemberParameterCount = parameter.DeclaringMethodParameterCount;
                }
                else if (parameter.DeclaringMethodParameterCount != declaringMemberParameterCount)
                {
                    ArgumentExceptionAdvanced.ThrowIfFalse(
                        false,
                        argumentName,
                        $"All items in the argument sequence '{argumentName}' must have the same value for the '{nameof(ParameterDescriptor)}.{nameof(ParameterDescriptor.DeclaringMethodParameterCount)}' property to avoid ambiguity.");
                }
            }
            else if (index > 0 && declaringMemberParameterCount != -1) // At least one previous item has a defined declaring method parameter count, but the current item does not have it defined, which leads to ambiguity and is therefore not allowed.
            {
                throw new ArgumentException(
                    $"All items in the argument sequence '{argumentName}' must have the exact same value for the '{nameof(ParameterDescriptor)}.{nameof(ParameterDescriptor.DeclaringMethodParameterCount)}' property to avoid ambiguity. The value must be either greater than zero or undefined (less than zero) and the same for all items.",
                    argumentName);
            }
        }

        if (declaringMemberParameterCount != ParameterDescriptor.UnknownParameterCountOrPosition)
        {
            // Allow incomplete parameter lists,
            // but ensure that the total count of parameters does not exceed the declaring member parameter count when specified.
            ArgumentExceptionAdvanced.ThrowIfTrue(
                Parameters.Count > declaringMemberParameterCount,
                argumentName,
                $"All items in the argument sequence '{argumentName}' must have a valid parameter position specified to avoid ambiguity.");
        }

        return declaringMemberParameterCount;
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
            ArgumentExceptionAdvanced.ThrowIfContainsDuplicate(
                Parameters,
                s_parameterEqualityComparer,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a duplicate value for the '{nameof(ParameterDescriptor)}.{nameof(ParameterDescriptor.ParameterPosition)}' parameter position or '{nameof(ParameterDescriptor)}.{nameof(ParameterDescriptor.ParameterName)}' parameter name.");

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

    public bool IsSortedByParameterPosition => _isSortedByParameterPosition
        ??= Parameters.All(parameter => parameter.HasParameterPosition);

    public int Count => Parameters.Count;
    public int DeclaringMethodParameterCount { get; }
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

        if (Count != other.Count
            || DeclaringMethodParameterCount != other.DeclaringMethodParameterCount)
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
            hashCode.Add(DeclaringMethodParameterCount);
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
