namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

/// <summary>
/// A descriptor that provides information about an anonymous method (anonymous is when the caller does not have a direct a <see cref="MethodInfo"/> representation of the method symbol).
/// </summary>
/// <remarks>The <see cref="AnonymousMethodDescriptor"/> is used to provide information for anonymous method symbols, which is when the caller does not have the direct <see cref="MethodInfo"/> representation.
/// <para/>When the caller has the direct <see cref="MethodInfo"/> representation and the method is well-known, use the <see cref="WellKnownMethodDescriptor"/> instead.
/// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="MethodInfo"/> is available to ensure maximum accuracy and performance.
/// </remarks>
internal readonly struct AnonymousMethodDescriptor : IEquatable<AnonymousMethodDescriptor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnonymousMethodDescriptor"/> struct.
    /// </summary>
    /// <remarks>The <see cref="AnonymousMethodDescriptor"/> is used to provide information about a method symbol of which the caller does not have a direct representation <see cref="MethodInfo"/> or <see cref="MethodData"/> and instead only signature information is available.
    /// <para/>If the method is an explicit interface implementation, then the <paramref name="implementingTypeHandle"/> must provide a <see cref="RuntimeTypeHandle"/> that refers to the originally declaring interface type.
    /// <para/>For best accuracy and performance always use the <see cref="WellKnownMethodDescriptor"/>, which requires the caller to have direct access to the <see cref="MethodInfo"/> or <see cref="ConstructorInfo"/> representation of the method or constructor.
    /// </remarks>
    /// <param name="declaringTypeHandle">The runtime type handle representing the implementing type of the anonymous method or constructor.</param>
    /// <param name="methodName">Conditionally optional. The name of the anonymous method. For methods the value cannot be null, empty, or consist only of white-space characters.
    /// <para/>For constructors this parameter is ignored (constructors don't have names).</param>
    /// <param name="methodParameters">The list of parameters for the anonymous method. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
    /// <param name="genericMethodParameters">The list of generic method parameters for the anonymous method.<para/>
    /// Can be <see cref="TypeList.Empty"/> for constructors or to indicate a non-generic method.
    /// <para/>For  constructors this parameter is ignored.</param>
    /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the method is an explicit interface implementation; otherwise, <see langword="false"/>.
    /// <para/>If set to <see langword="true"/> and the descriptor describes an explicitly implemented method,
    /// then the <paramref name="implementingTypeHandle"/> must provide a <see cref="RuntimeTypeHandle"/> that was obtained from the declaring interface type.</param>
    /// <param name="implementingTypeHandle">The runtime type handle representing the declaring interface type of the anonymous method.
    /// <para/>Must be provided when the method is an explicit interface implementation (which is when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/>).
    /// <br/>Otherwise, this parameter can be <see langword="null"/> and will be ignored.</param>
    /// <returns>A new instance of <see cref="AnonymousMethodDescriptor"/> representing the specified anonymous method.</returns>
    /// <exception cref="ArgumentNullException">Thrown when
    /// <list type="bullet">
    /// <item><paramref name="declaringTypeHandle"/> is <see langword="default"/>.</item>
    /// <item><paramref name="methodName"/> is <see langword="null"/>, empty, or consists only of white-space characters.</item>
    /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="implementingTypeHandle"/> is <see langword="null"/>.</item>
    /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="implementingTypeHandle"/> is <see langword="default"/>.</item>
    /// </list>
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when
    /// <list type="bullet">
    /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="declaringTypeHandle"/> was not obtained from an interface type.</item>
    /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="implementingTypeHandle"/> was obtained from an interface type.</item>
    /// <item>the provided <paramref name="declaringTypeHandle"/> refers to an interface type.</item>
    /// </list>
    /// </exception>
    public AnonymousMethodDescriptor(
        RuntimeTypeHandle declaringTypeHandle,
        ParameterDescriptorList? methodParameters,
        string? methodName,
        ITypeListView? genericMethodParameters,
        bool isExplicitInterfaceImplementation,
        RuntimeTypeHandle? implementingTypeHandle)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
        var declaringType = Type.GetTypeFromHandle(declaringTypeHandle);
        ArgumentNullExceptionAdvanced.ThrowIfNull(
            declaringType,
            nameof(declaringTypeHandle),
            $"Invalid argument '{nameof(declaringTypeHandle)}'. The provided declaring type handle does not resolve to a runtime type.");

        ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(methodName);

        if (isExplicitInterfaceImplementation)
        {
            ArgumentExceptionAdvanced.ThrowIfFalse(
                declaringType!.IsInterface,
                nameof(declaringTypeHandle),
                $"Invalid argument '{nameof(declaringTypeHandle)}'. The argument '{nameof(declaringType)}' is pointing to an non-interface type. Reason: Only interface types can declare explicit member implementations.");

            ArgumentNullExceptionAdvanced.ThrowIfNull(
                implementingTypeHandle,
                nameof(implementingTypeHandle),
                $"Invalid argument '{nameof(implementingTypeHandle)}'. The provided declaring interface type handle is not 'NULL' which is not allowed for explicit interface implementations (which is when '{nameof(isExplicitInterfaceImplementation)}' is 'true').");
            ArgumentNullExceptionAdvanced.ThrowIfDefault(implementingTypeHandle!.Value);
            var declaringInterfaceType = Type.GetTypeFromHandle(implementingTypeHandle!.Value);
            ArgumentNullExceptionAdvanced.ThrowIfNull(
                declaringInterfaceType,
                nameof(implementingTypeHandle),
                $"The declaring interface type represented by the argument '{nameof(implementingTypeHandle)}' could not be resolved.");
            ArgumentExceptionAdvanced.ThrowIfTrue(declaringInterfaceType!.IsInterface,
                nameof(isExplicitInterfaceImplementation),
                $"Invalid argument '{nameof(implementingTypeHandle)}'. The argument '{nameof(implementingTypeHandle)}' points to a interface type. Reason: Only non-interface types can provide the explicit interface implementations.");
        }

        DeclaringInterfaceTypeHandle = isExplicitInterfaceImplementation
            ? implementingTypeHandle!.Value
            : default;
        ImplementingTypeHandle = declaringTypeHandle;
        MethodName = methodName;
        MethodParameterList = methodParameters!.OrEmpty();
        GenericMethodParameters = genericMethodParameters.OrEmpty();
        IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
        IsAnonymous = true;
    }

    public RuntimeTypeHandle ImplementingTypeHandle { get; }
    public RuntimeTypeHandle DeclaringInterfaceTypeHandle { get; }
    public string MethodName { get; }
    public ParameterDescriptorList MethodParameterList { get; }
    public TypeList GenericMethodParameters { get; }
    public bool IsExplicitInterfaceImplementation { get; }
    public bool IsAnonymous { get; }

    public bool Equals(AnonymousMethodDescriptor other) => ImplementingTypeHandle.Equals(other.ImplementingTypeHandle)
        && DeclaringInterfaceTypeHandle.Equals(other.DeclaringInterfaceTypeHandle)
        && MethodName.Equals(other.MethodName, StringComparison.Ordinal)
        && GenericMethodParameters.Equals(other.GenericMethodParameters)
        && IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
        && MethodParameterList.Equals(other.MethodParameterList)
        && IsAnonymous == other.IsAnonymous;

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(ImplementingTypeHandle);
        hashCode.Add(DeclaringInterfaceTypeHandle);
        hashCode.Add(MethodName);
        hashCode.Add(GenericMethodParameters);
        hashCode.Add(IsExplicitInterfaceImplementation);
        hashCode.Add(MethodParameterList);
        hashCode.Add(IsAnonymous);

        return hashCode.ToHashCode();
    }

    public static bool operator ==(AnonymousMethodDescriptor left, AnonymousMethodDescriptor right) => left.Equals(right);
    public static bool operator !=(AnonymousMethodDescriptor left, AnonymousMethodDescriptor right) => !(left == right);

    public override bool Equals(object? obj) => obj is AnonymousMethodDescriptor other && Equals(other);
}
