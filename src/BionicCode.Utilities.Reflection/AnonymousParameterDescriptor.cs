namespace BionicCode.Utilities.Net.Reflection
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides the specified parameter information for an anonymous parameter symbol.
    /// </summary>
    /// <remarks>The <see cref="AnonymousParameterDescriptor"/> is used to provide information for anonymous parameter symbols, which is when the caller does not have the direct <see cref="ParameterInfo"/> representation.
    /// <para/>When the caller has the direct <see cref="ParameterInfo"/> representation and the method is well-known, use the <see cref="WellKnownParameterDescriptor"/> instead.
    /// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="ParameterInfo"/> is available to ensure maximum accuracy and performance.
    /// </remarks>
    internal readonly struct AnonymousParameterDescriptor : IEquatable<AnonymousParameterDescriptor>
    {
        /// <summary>
        /// Constructs a descriptor that provides the specified parameter information for an anonymous parameter.
        /// </summary>
        /// <param name="declaringMethodDescriptor">The <see cref="AnonymousMethodDescriptor"/> representing the anonymous method that declares the parameter.</param>
        /// <param name="parameterTypeHandle">Conditionally optional. The runtime type handle representing the type of the anonymous parameter.<para/>
        /// Must be provided if all of the following arguments are missing: <paramref name="parameterName"/> AND <paramref name="parameterKind"/> AND <paramref name="parameterPosition"/>.</param>
        /// <param name="parameterName">Conditionally optional. The name of the anonymous parameter.<para/>
        /// Must be provided if all of the following arguments are missing: <paramref name="parameterTypeHandle"/> AND <paramref name="parameterKind"/> AND <paramref name="parameterPosition"/>.</param>
        /// <param name="parameterPosition">Conditionally optional.The index of the parameter.<para/>
        /// Must be provided if all of the following arguments are missing: <paramref name="parameterName"/> AND <paramref name="parameterTypeHandle"/> AND <paramref name="parameterKind"/>.</param>
        /// <param name="parameterKind">Conditionally optional. The modifier of the parameter.<para/>
        /// Must be provided if all of the following arguments are missing: <paramref name="parameterName"/> AND <paramref name="parameterTypeHandle"/> AND <paramref name="parameterPosition"/>.</param>
        public AnonymousParameterDescriptor(
            AnonymousMethodDescriptor declaringMethodDescriptor,
            string? parameterName = null,
            int parameterPosition = SymbolReflectionInfoCacheKeyInternal.UnknownParameterCountOrPosition,
            ParameterKind parameterKind = ParameterKind.Undefined,
            RuntimeTypeHandle? parameterTypeHandle = null)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<ParameterKind>(parameterKind);
            if (parameterTypeHandle.Equals(default)
                && string.IsNullOrWhiteSpace(parameterName)
                && parameterKind == ParameterKind.Undefined
                && parameterPosition == SymbolReflectionInfoCacheKeyInternal.UnknownParameterCountOrPosition)
            {
                throw new ArgumentException($"At least one of the following arguments must be provided to avoid ambiguity when using the created key for lookups: '{nameof(parameterTypeHandle)}', '{nameof(parameterName)}', '{nameof(parameterKind)}', '{nameof(parameterPosition)}'.");
            }

            DeclaringMethodDescriptor = declaringMethodDescriptor;
            ParameterName = parameterName ?? string.Empty;
            ParameterPosition = parameterPosition;
            ParameterKind = parameterKind;
            ParameterTypeHandle = parameterTypeHandle ?? default;
        }

        public bool HasParameterName
            => !string.IsNullOrWhiteSpace(ParameterName);

        public bool HasParameterPosition
            => ParameterPosition > SymbolReflectionInfoCacheKeyInternal.UnknownParameterCountOrPosition;

        public bool HasParameterKind
            => ParameterKind != ParameterKind.Undefined;

        public bool HasParameterTypeHandle
            => !ParameterTypeHandle.Equals(default);

        public bool IsAnonymous
            => true;

        public AnonymousMethodDescriptor DeclaringMethodDescriptor { get; }
        public string ParameterName { get; }
        public int ParameterPosition { get; }
        public ParameterKind ParameterKind { get; }
        public RuntimeTypeHandle ParameterTypeHandle { get; }

        public bool Equals(AnonymousParameterDescriptor other) => ParameterName.Equals(other.ParameterName, StringComparison.Ordinal)
            && ParameterPosition == other.ParameterPosition
            && ParameterKind == other.ParameterKind
            && ParameterTypeHandle.Equals(other.ParameterTypeHandle)
            && DeclaringMethodDescriptor.Equals(other.DeclaringMethodDescriptor);

        public override int GetHashCode() => HashCode.Combine(
            ParameterName,
            ParameterPosition,
            ParameterKind,
            ParameterTypeHandle,
            DeclaringMethodDescriptor);

        public static bool operator ==(AnonymousParameterDescriptor left, AnonymousParameterDescriptor right)
            => left.Equals(right);
        public static bool operator !=(AnonymousParameterDescriptor left, AnonymousParameterDescriptor right)
            => !left.Equals(right);

        public override bool Equals(object? obj)
            => obj is AnonymousParameterDescriptor other && Equals(other);
    }
}
