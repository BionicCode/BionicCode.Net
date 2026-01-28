namespace BionicCode.Utilities.Net
{
    using System;

    /// <summary>
    /// A descriptor that provides the specified parameter information for an anonymous parameter cache key
    /// </summary>
    internal readonly struct CacheKeyParameterDescriptor : IEquatable<CacheKeyParameterDescriptor>
    {
        /// <summary>
        /// Constructs a descriptor that provides the specified parameter information for an anonymous parameter cache key
        /// </summary>
        /// <param name="parameterTypeHandle">Conditionally optional. The runtime type handle representing the type of the anonymous parameter.<para/>
        /// Must be provided if all of the following arguments are missing: <paramref name="parameterName"/> AND <paramref name="parameterKind"/> AND <paramref name="parameterPosition"/>.</param>
        /// <param name="parameterName">Conditionally optional. The name of the anonymous parameter.<para/>
        /// Must be provided if all of the following arguments are missing: <paramref name="parameterTypeHandle"/> AND <paramref name="parameterKind"/> AND <paramref name="parameterPosition"/>.</param>
        /// <param name="parameterPosition">Conditionally optional.The index of the parameter.<para/>
        /// Must be provided if all of the following arguments are missing: <paramref name="parameterName"/> AND <paramref name="parameterTypeHandle"/> AND <paramref name="parameterKind"/>.</param>
        /// <param name="parameterKind">Conditionally optional. The modifier of the parameter.<para/>
        /// Must be provided if all of the following arguments are missing: <paramref name="parameterName"/> AND <paramref name="parameterTypeHandle"/> AND <paramref name="parameterPosition"/>.</param>
        public CacheKeyParameterDescriptor(string? parameterName = null,
            int parameterPosition = SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
            ParameterKind parameterKind = ParameterKind.Undefined,
            RuntimeTypeHandle? parameterTypeHandle = null)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<ParameterKind>(parameterKind);
            if (parameterTypeHandle.Equals(default)
                && string.IsNullOrWhiteSpace(parameterName)
                && parameterKind == ParameterKind.Undefined
                && parameterPosition == SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition)
            {
                throw new ArgumentException($"At least one of the following arguments must be provided to avoid ambiguity when using the created key for lookups: '{nameof(parameterTypeHandle)}', '{nameof(parameterName)}', '{nameof(parameterKind)}', '{nameof(parameterPosition)}'.");
            }

            this.ParameterName = parameterName ?? string.Empty;
            this.ParameterPosition = parameterPosition;
            this.ParameterKind = parameterKind;
            this.ParameterTypeHandle = parameterTypeHandle ?? default;
        }

        public bool HasParameterName
            => !string.IsNullOrWhiteSpace(this.ParameterName);

        public bool HasParameterPosition
            => this.ParameterPosition > SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition;

        public bool HasParameterKind
            => this.ParameterKind != ParameterKind.Undefined;

        public bool HasParameterTypeHandle
            => !this.ParameterTypeHandle.Equals(default);

        public string ParameterName { get; init; }
        public int ParameterPosition { get; init; }
        public ParameterKind ParameterKind { get; init; }
        public RuntimeTypeHandle ParameterTypeHandle { get; init; }

        public bool Equals(CacheKeyParameterDescriptor other) => this.ParameterName.Equals(other.ParameterName, StringComparison.Ordinal)
            && this.ParameterPosition == other.ParameterPosition
            && this.ParameterKind == other.ParameterKind
            && this.ParameterTypeHandle.Equals(other.ParameterTypeHandle);

        public override int GetHashCode() => HashCode.Combine(
            this.ParameterName,
            this.ParameterPosition,
            this.ParameterKind,
            this.ParameterTypeHandle);

        public static bool operator ==(CacheKeyParameterDescriptor left, CacheKeyParameterDescriptor right)
            => left.Equals(right);
        public static bool operator !=(CacheKeyParameterDescriptor left, CacheKeyParameterDescriptor right)
            => !left.Equals(right);

        public override bool Equals(object? obj)
            => obj is CacheKeyParameterDescriptor other && Equals(other);
    }
}
