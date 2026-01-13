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
        /// <summary>
        /// Gets a value indicating whether the type parameter is declared by a generic method definition.
        /// </summary>
        /// <value><see langword="true"/> if the type parameter is declared by a generic method definition; otherwise, <see langword="false"/>.</value>
        public bool IsGenericMethodParameter { get; }

        /// <summary>
        /// Provides information about a method parameter.
        /// </summary>
        /// <param name="parameterTypeHandle">The runtime type handle of the parameter's type.</param>
        /// <param name="position">The position of the parameter in the method signature.</param>
        /// <param name="isGenericMethodParameter">Indicates whether the parameter is a generic method parameter.</param>
        /// <param name="kind">The kind of the parameter (e.g., input, output).</param>
        /// <param name="declaringTypeHandle">The runtime type handle of the type that declares the method that defines the parameter.</param>
        public MethodParameterInfo(SymbolInfoDataCacheKey parameterInfoDataCacheKey) : this()
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(parameterInfoDataCacheKey, nameof(parameterInfoDataCacheKey));
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                parameterInfoDataCacheKey.ParameterKind,
                [ParameterKind.Undefined],
                nameof(parameterInfoDataCacheKey),
                $"The property '{nameof(MethodParameterInfo.Kind)}' of the argument '{parameterInfoDataCacheKey}' cannot be of value '{nameof(ParameterKind)}.{nameof(ParameterKind.Undefined)}'.");
            this.Position = parameterInfoDataCacheKey.ParameterPosition;
            this.IsGenericMethodParameter = parameterInfoDataCacheKey;
            this.ParameterTypeHandle = parameterInfoDataCacheKey.SymbolTypeHandle;
            this.Kind = parameterInfoDataCacheKey.ParameterKind;
            this.DeclaringTypeHandle = parameterInfoDataCacheKey.DeclaringTypeHandle;
        }

        public override bool Equals(object? obj) => obj is MethodParameterInfo info && Equals(info);
        public bool Equals(MethodParameterInfo other) => other.ParameterTypeHandle.Equals(this.ParameterTypeHandle)
            && other.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle)
            && other.Position == this.Position
            && other.Kind.Equals(this.Kind)
            && other.IsGenericMethodParameter.Equals(this.IsGenericMethodParameter);

        public override int GetHashCode()
            => HashCode.Combine(this.ParameterTypeHandle, this.DeclaringTypeHandle, this.Position, this.Kind, this.IsGenericMethodParameter);
    }
}
