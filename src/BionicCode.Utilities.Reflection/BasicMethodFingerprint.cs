namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Provides a basic fingerprint for a method based on its name, declaring type, return type, parameters and generic method parameters.
/// </summary>
/// <remarks>The <see cref="BasicMethodFingerprint"/> allows to identify methods based on the least required attributes.
/// This allows to identify methods that belong to the same generic family (based on the open generic method signature).<br/>
/// For example, the open generic method would share the same basic fingerprint like all  it's closed generic variants.<br/>
/// In other words, the <see cref="BasicMethodFingerprint"/> enables the identification of generic methods that share the same generic structure since it is based on the raw unconstructed generic method signature.</remarks>
internal readonly struct BasicMethodFingerprint : IEquatable<BasicMethodFingerprint>
{
    public BasicMethodFingerprint(string methodName, RuntimeTypeHandle declaringTypeHandle, RuntimeTypeHandle returnTypeHandle, ParameterList parameters, TypeList genericMethodParameters)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(methodName, nameof(methodName));
        ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
        ArgumentNullExceptionAdvanced.ThrowIfDefault(returnTypeHandle, nameof(returnTypeHandle));
        ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));
        ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

        MethodName = methodName;
        DeclaringTypeData = declaringTypeHandle;
        ReturnTypeData = returnTypeHandle;
        Parameters = parameters;
        GenericMethodParameters = genericMethodParameters;

        _hashCode = ComputeHashCode();
    }

    public string MethodName { get; }
    public RuntimeTypeHandle DeclaringTypeData { get; }
    public RuntimeTypeHandle ReturnTypeData { get; }
    public ParameterList Parameters { get; }
    public TypeList GenericMethodParameters { get; }
    private readonly int _hashCode;

    public bool Equals(BasicMethodFingerprint other) => MethodName.Equals(other.MethodName, StringComparison.Ordinal)
        && DeclaringTypeData.Equals(other.DeclaringTypeData)
        && ReturnTypeData.Equals(other.ReturnTypeData)
        && Parameters == other.Parameters
        && GenericMethodParameters == other.GenericMethodParameters;

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is BasicMethodFingerprint basicMethodFingerprint && Equals(basicMethodFingerprint);
    public override int GetHashCode()
        => _hashCode;

    private int ComputeHashCode() => HashCode.Combine(
            MethodName,
            DeclaringTypeData,
            ReturnTypeData,
            Parameters,
            GenericMethodParameters);

    public static bool operator ==(BasicMethodFingerprint left, BasicMethodFingerprint right) => left.Equals(right);
    public static bool operator !=(BasicMethodFingerprint left, BasicMethodFingerprint right) => !(left == right);
}
