namespace BionicCode.Utilities.Net;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// A collection of extension methods for various default constraintTypes
/// </summary>
public static partial class HelperExtensionsCommon
{
    /// <summary>
    /// Converts a <see cref="Predicate{T}"/> to a <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <typeparam genericTypeParameterIdentifier="TParam">The parameter valueType for the predicate.</typeparam>
    /// <param genericTypeParameterIdentifier="predicate">The predicate to convert.</param>
    /// <returns>A <c>Func<typeparamref genericTypeParameterIdentifier="TParam"/>, bool></c> that returns the result of <paramref genericTypeParameterIdentifier="predicate"/>.</returns>
    public static Func<TParam, bool> ToFunc<TParam>(this Predicate<TParam> predicate)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(predicate);

        return predicate.Invoke;
    }

    internal static StringBuilder AppendStringBuilder(this StringBuilder stringBuilder, StringBuilder value)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(stringBuilder);
        ArgumentNullExceptionAdvanced.ThrowIfNull(value);

        return stringBuilder.Append(value);
    }

    /// <summary>
    /// Returns the innermost delegate by unwrapping any nested delegate wrappers from the specified delegate.
    /// </summary>
    /// <remarks>This method is useful when working with delegates that may have been wrapped by other
    /// delegates, such as when using certain proxy or interception frameworks. It recursively unwraps delegates
    /// until the original, non-wrapped delegate is found.</remarks>
    /// <param name="delegateInstance">The delegate to unwrap. This may be a delegate that wraps another delegate as its target.</param>
    /// <returns>The innermost delegate that is not itself wrapping another delegate. If the input delegate does not wrap
    /// another delegate, the same instance is returned.</returns>
    public static Delegate UnwrapDelegate(this Delegate delegateInstance)
    {
        ArgumentNullException.ThrowIfNull(delegateInstance);

        object? dTarget = delegateInstance.Target;

        // Unwrap delegate if wrapped
        while (dTarget is Delegate dTemp)
        {
            delegateInstance = dTemp;
            dTarget = delegateInstance.Target;
        }

        return delegateInstance;
    }

    /// <summary>
    /// A performance sensitive extension method that determines whether two value type instances are equal using the default equality comparer.
    /// </summary>
    /// <remarks>This method helps to avoid the performance costs of <see cref="ValueType.Equals(object?)"/> which includes boxing and reflection-based field-by-field comparison. However, <see cref="ValueEquals{TStruct}(TStruct, TStruct)"/> uses the default equality comparer for the specified value type, which allows to use the <see cref="IEquatable{T}"/> implementation instead.<para/>
    /// For custom equality logic, ensure that the struct implements <see cref="IEquatable{T}"/> to avoid boxing in addition to providing a correct way of equality comparison, or at least overrides <see cref="object.Equals(object?)"/> and
    /// GetHashCode appropriately.</remarks>
    /// <typeparam name="TStruct">The value type to compare. Must be a struct.</typeparam>
    /// <param name="structInstance">The first value type instance to compare.</param>
    /// <param name="otherStructInstance">The second value type instance to compare.</param>
    /// <returns>true if the two instances are considered equal; otherwise, false.</returns>
    public static bool ValueTypeEquals<TStruct>(this TStruct structInstance, TStruct otherStructInstance)
        where TStruct : struct
        => EqualityComparer<TStruct>.Default.Equals(structInstance, otherStructInstance);
}
