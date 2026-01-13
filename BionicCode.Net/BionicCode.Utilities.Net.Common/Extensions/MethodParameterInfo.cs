namespace BionicCode.Utilities.Net
{
    using System;

    internal readonly struct MethodParameterInfo : IEquatable<MethodParameterInfo>
    {
        /// <summary>
        /// Gets the zero based position of the parameter in the method signature.
        /// </summary>
        /// <value>The zero based position of the parameter in the method signature.</value>
        public int Position { get; }
        /// <summary>
        /// Gets the runtime handle for the parameter's type.
        /// </summary>
        public RuntimeTypeHandle ParameterTypeHandle { get; }
        /// <summary>
        /// The runtime type handle of the type that declares the method that defines the parameter.
        /// </summary>
        public RuntimeTypeHandle DeclaringTypeHandle { get; }
        /// <summary>
        /// Describes the modifier kind of the parameter (e.g. <see langword="ref"/>).
        /// </summary>
        public ParameterKind Kind { get; }

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

            this.Position = parameterData.Position;
            //this.IsGenericMethodParameter = parameterInfoDataCacheKey;
            this.ParameterTypeHandle = parameterData.ParameterTypeHandle;
            this.Kind = parameterData.ParameterKind;
            this.DeclaringTypeHandle = parameterData.DeclaringTypeHandle;
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
        public MethodParameterInfo(SymbolInfoDataCacheKey parameterInfoDataCacheKey) : this()
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(parameterInfoDataCacheKey, nameof(parameterInfoDataCacheKey));
            ArgumentNullExceptionAdvanced.ThrowIfDefault(parameterInfoDataCacheKey.DeclaringTypeHandle, nameof(parameterInfoDataCacheKey));
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                parameterInfoDataCacheKey.ParameterKind,
                [ParameterKind.Undefined],
                nameof(parameterInfoDataCacheKey),
                $"The property '{nameof(MethodParameterInfo.Kind)}' of the argument '{parameterInfoDataCacheKey}' cannot be of value '{nameof(ParameterKind)}.{nameof(ParameterKind.Undefined)}'.");
            this.Position = parameterInfoDataCacheKey.ParameterPosition;
            //this.IsGenericMethodParameter = parameterInfoDataCacheKey;
            this.ParameterTypeHandle = parameterInfoDataCacheKey.SymbolTypeHandle;
            this.Kind = parameterInfoDataCacheKey.ParameterKind;
            this.DeclaringTypeHandle = parameterInfoDataCacheKey.DeclaringTypeHandle;
        }

        public override bool Equals(object? obj) => obj is MethodParameterInfo info && Equals(info);
        public bool Equals(MethodParameterInfo other) => other.ParameterTypeHandle == this.ParameterTypeHandle
            && other.DeclaringTypeHandle == this.DeclaringTypeHandle
            && other.Position == this.Position
            && other.Kind == this.Kind;
        //&& other.IsGenericMethodParameter.Equals(this.IsGenericMethodParameter);

        public override int GetHashCode()
            => HashCode.Combine(this.ParameterTypeHandle, this.DeclaringTypeHandle, this.Position, this.Kind);

        public static bool operator ==(MethodParameterInfo left, MethodParameterInfo right) => left.Equals(right);
        public static bool operator !=(MethodParameterInfo left, MethodParameterInfo right) => !(left == right);
    }
}
