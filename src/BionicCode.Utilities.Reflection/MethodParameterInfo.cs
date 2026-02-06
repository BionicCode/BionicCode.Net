namespace BionicCode.Utilities.Net.Reflection
{
    using System;

    internal readonly struct MethodParameterInfo : IEquatable<MethodParameterInfo>
    {
        public WellKnownParameterDescriptor ParameterDescriptor { get; }
        public ParameterMemberDescriptor DeclaringMemberDescriptor { get; }

        /// <summary>
        /// Gets a value indicating whether ambiguity is expected for the associated parameter or member descriptor.
        /// </summary>
        /// <remarks>Ambiguity is expected when neither the parameter nor the declaring member can be
        /// uniquely identified by name, position, or handle. This property can be used to determine if additional
        /// disambiguation logic may be required when resolving parameters or members.</remarks>
        public bool IsAmbiguityExpected
            => (!ParameterDescriptor.HasParameterName
                && !ParameterDescriptor.HasParameterPosition)
                || !DeclaringMemberDescriptor.HasMemberHandle;

        ///// <summary>
        ///// Gets a value indicating whether the type parameter is declared by a generic method definition.
        ///// </summary>
        ///// <value><see langword="true"/> if the type parameter is declared by a generic method definition; otherwise, <see langword="false"/>.</value>
        //public bool IsGenericMethodParameter { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodParameterInfo"/> class using the specified parameter metadata.
        /// </summary>
        /// <param name="parameterData">The <see cref="ParameterData"/> describing the parameter, including its position, type, kind, and declaring type. Cannot be
        /// null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameterData"/> is <see langword="null"/>.</exception>
        public MethodParameterInfo(ParameterData parameterData) : this()
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(parameterData);

            RuntimeTypeHandle memberTypeHandle = parameterData.MemberData is MethodData methodData
                ? methodData.ReturnTypeData.Handle
                : default;

            ParameterDescriptor = new CacheKeyWellKnownParameterDescriptor(
                parameterData.Name,
                parameterData.Position,
                parameterData.ParameterKind,
                parameterData.ParameterTypeHandle);
            DeclaringMemberDescriptor = new ParameterMemberDescriptor(
                parameterData.DeclaringTypeHandle,
                parameterData.MemberData.Handle,
                parameterData.MemberData.Name,
                parameterData.MemberData.Parameters.Count,
                parameterData.MemberData.ParameterizedSymbolKind,
                TypeList.Empty,
                memberTypeHandle);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodParameterInfo"/> class using the specified symbol information data
        /// cache key.
        /// </summary>
        /// <remarks>Throws an exception if the provided cache key is <see langword="default"/>, its declaring type handle is
        /// <see langword="default"/>, or its parameter kind is <see cref="ParameterKind.Undefined"/>. This constructor is intended for scenarios where parameter
        /// metadata is anonymously available in a precomputed implicit form. If the explicit metadata of this parameter in form of a <see cref="ParameterData"/> is available, it should be used with the overload <see cref="MethodParameterInfo(ParameterData)"/> instead.</remarks>
        /// <param name="parameterInfoDataCacheKey">An object containing metadata about the method parameter, including its position, type, kind, and declaring
        /// type. Cannot be <see langword="default"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameterInfoDataCacheKey"/> is <see langword="default"/> or its declaring type handle is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <see cref="ParameterKind"/> of <paramref name="parameterInfoDataCacheKey"/> is <see cref="ParameterKind.Undefined"/>.</exception>"
        public MethodParameterInfo(SymbolReflectionInfoCacheKey parameterInfoDataCacheKey) : this()
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(parameterInfoDataCacheKey.ParameterDescriptor, nameof(parameterInfoDataCacheKey));
            ArgumentNullExceptionAdvanced.ThrowIfDefault(parameterInfoDataCacheKey.ParameterMemberDescriptor, nameof(parameterInfoDataCacheKey));

            WellKnownParameterDescriptor parameterDescriptor = parameterInfoDataCacheKey.ParameterDescriptor;
            ParameterMemberDescriptor declaringMemberDescriptor = parameterInfoDataCacheKey.ParameterMemberDescriptor;
            ParameterDescriptor = parameterDescriptor;
            DeclaringMemberDescriptor = declaringMemberDescriptor;
        }

        public override bool Equals(object? obj) => obj is MethodParameterInfo info && Equals(info);
        public bool Equals(MethodParameterInfo other) => ParameterDescriptor == other.ParameterDescriptor
            && DeclaringMemberDescriptor == other.DeclaringMemberDescriptor;

        public override int GetHashCode()
            => HashCode.Combine(ParameterDescriptor, DeclaringMemberDescriptor);

        public static bool operator ==(MethodParameterInfo left, MethodParameterInfo right) => left.Equals(right);
        public static bool operator !=(MethodParameterInfo left, MethodParameterInfo right) => !(left == right);
    }
}
