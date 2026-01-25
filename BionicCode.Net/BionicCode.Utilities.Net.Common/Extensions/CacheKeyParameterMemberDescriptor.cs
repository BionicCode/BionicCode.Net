namespace BionicCode.Utilities.Net
{
    using System;

    /// <summary>
    /// A descriptor that provides information about the member that declares the parameter for an anonymous parameter cache key.
    /// </summary>
    internal readonly struct CacheKeyParameterMemberDescriptor : IEquatable<CacheKeyParameterMemberDescriptor>
    {
        /// <summary>
        /// Constructs a descriptor that provides information about the member that declares the parameter for an anonymous parameter cache key.
        /// </summary>
        /// <param name="memberGenericMethodParameters">Optional. The list of generic method parameters for the anonymous parameter type.<para/>
        /// Can be <see cref="MethodParameterInfoList.Empty"/> to indicate a non-generic parameter or unknown. Providing <paramref name="memberGenericMethodParameters"/> helps to avoid ambiguity.</param>
        /// <param name="memberName">
        /// Optional. The name of the member that declares the parameter. Providing this information can help to avoid ambiguity and improve lookup performance.
        /// <para/>Will be ignored when <paramref name="parameterizedSymbolKind"/> is <see cref="ParameterizedSymbolKind.MemberConstructor"/> or when the <paramref name="parameterizedSymbolKind"/> indicates an indexer property (<see cref="ParameterizedSymbolKind.MemberIndexerPropertyGet"/> or <see cref="ParameterizedSymbolKind.MemberIndexerPropertySet"/>).
        /// </param>
        /// <param name="memberParameterCount">Optional. The number of parameters for the member that declares the parameter. Provide to narrow down ambiguity and improve performance.</param>
        /// <param name="parameterizedSymbolKind">Optional. Provides a hint about the kind of member that the parameter belongs to. Should be provided too improve efficiency of the key and to avoid ambiguity.</param>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the member that defines the anonymous parameter.</param>
        /// <param name="memberHandle">Optional. For best performance provide a <see cref="RuntimeMethodHandle"/> to the method or constructor that defines the parameter. 
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is the default value.</exception>
        public CacheKeyParameterMemberDescriptor(RuntimeTypeHandle declaringTypeHandle,
            RuntimeMethodHandle? memberHandle = null,
            string? memberName = null,
            int memberParameterCount = SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
            ParameterizedSymbolKind parameterizedSymbolKind = ParameterizedSymbolKind.Undefined,
            TypeList? memberGenericMethodParameters = null,
            RuntimeTypeHandle? propertyTypeHandle = null)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<ParameterizedSymbolKind>(parameterizedSymbolKind);
            if (parameterizedSymbolKind == ParameterizedSymbolKind.MemberIndexerPropertyGet
                || parameterizedSymbolKind == ParameterizedSymbolKind.MemberIndexerPropertySet
                || parameterizedSymbolKind == ParameterizedSymbolKind.MemberConstructor)
            {
                // For indexer properties and constructors we allow empty or whitespace names i.e. ignore provided value.
                memberName = string.Empty;
            }

            this.DeclaringMemberName = memberName ?? string.Empty;
            this.MemberParameterCount = memberParameterCount;
            this.ParameterizedMemberKind = parameterizedSymbolKind;
            this.MemberGenericMethodParameters = memberGenericMethodParameters ?? TypeList.Empty;
            this.DeclaringTypeHandle = declaringTypeHandle;
            this.MemberHandle = memberHandle ?? default;
            this.PropertyTypeHandle = propertyTypeHandle ?? default;
        }

        public bool HasDeclaringMemberName
            => !string.IsNullOrWhiteSpace(this.DeclaringMemberName);

        public bool HasMemberGenericMethodParameters
            => !this.MemberGenericMethodParameters.IsEmpty;

        public bool HasMemberHandle
            => !this.MemberHandle.Equals(default);

        public bool HasMemberParameterCount
            => this.MemberParameterCount > SymbolInfoDataCacheKey.UnknownParameterCountOrPosition;

        public bool HasDeclaringTypeHandle
            => !this.DeclaringTypeHandle.Equals(default);

        public bool HasParameterizedMemberKind
            => this.ParameterizedMemberKind != ParameterizedSymbolKind.Undefined;

        public bool HasPropertyTypeHandle
            => !this.PropertyTypeHandle.Equals(default);

        public string DeclaringMemberName { get; init; }
        public int MemberParameterCount { get; init; }
        public ParameterizedSymbolKind ParameterizedMemberKind { get; init; }
        public TypeList MemberGenericMethodParameters { get; init; }
        public RuntimeTypeHandle DeclaringTypeHandle { get; init; }
        public RuntimeTypeHandle PropertyTypeHandle { get; init; }
        public RuntimeMethodHandle MemberHandle { get; }

        public bool Equals(CacheKeyParameterMemberDescriptor other) => this.DeclaringMemberName.Equals(other.DeclaringMemberName, StringComparison.Ordinal)
            && this.MemberParameterCount == other.MemberParameterCount
            && this.ParameterizedMemberKind == other.ParameterizedMemberKind
            && this.MemberGenericMethodParameters.Equals(other.MemberGenericMethodParameters)
            && this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && this.MemberHandle.Equals(other.MemberHandle);

        public override int GetHashCode() => HashCode.Combine(
            this.DeclaringMemberName,
            this.MemberParameterCount,
            this.ParameterizedMemberKind,
            this.MemberGenericMethodParameters,
            this.DeclaringTypeHandle,
            this.MemberHandle);

        public static bool operator ==(CacheKeyParameterMemberDescriptor left, CacheKeyParameterMemberDescriptor right) => left.Equals(right);
        public static bool operator !=(CacheKeyParameterMemberDescriptor left, CacheKeyParameterMemberDescriptor right) => !(left == right);

        public override bool Equals(object obj)
            => obj is CacheKeyParameterMemberDescriptor other && Equals(other);
    }
}
