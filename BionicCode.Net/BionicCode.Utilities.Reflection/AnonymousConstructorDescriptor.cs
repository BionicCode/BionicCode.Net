namespace BionicCode.Utilities.Net.Reflection
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides information about an anonymous constructor (where the caller does not have a direct <see cref="ConstructorInfo"/> representation of the constructor symbol).
    /// </summary>
    /// <remarks>The <see cref="AnonymousConstructorDescriptor"/> is used to provide information for anonymous constructor symbols, which is when the caller does not have the direct <see cref="ConstructorInfo"/> representation.
    /// <para/>When the caller has the direct <see cref="ConstructorInfo"/> representation and the constructor is well-known, use the <see cref="WellKnownConstructorDescriptor"/> instead.
    /// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="ConstructorInfo"/> is available to ensure maximum accuracy and performance.
    /// </remarks>
    internal readonly struct AnonymousConstructorDescriptor : IEquatable<AnonymousConstructorDescriptor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousConstructorDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="AnonymousConstructorDescriptor"/> is used to provide information about a constructor symbol of which the caller does not have a direct <see cref="ConstructorData"/> representation and instead only signature information is available.
        /// <para/>For best accuracy and performance always use the <see cref="WellKnownConstructorDescriptor"/>, which requires the caller to have direct access to the <see cref="MethodInfo"/> or <see cref="ConstructorInfo"/> representation of the method or constructor.
        /// </remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous constructor.</param>
        /// <param name="constructorParameters">The list of parameters for the anonymous method. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
        /// <returns>A new instance of <see cref="AnonymousConstructorDescriptor"/> representing the specified anonymous method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="declaringTypeHandle"/> is <see langword="default"/>.</item>
        /// <item>the provided <paramref name="declaringTypeHandle"/> does not resolve to a runtime type.</item>
        /// </list>
        /// </exception>
        public AnonymousConstructorDescriptor(
            RuntimeTypeHandle declaringTypeHandle,
            MethodParameterInfoList? constructorParameters)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            Type? declaringType = Type.GetTypeFromHandle(declaringTypeHandle);
            ArgumentNullExceptionAdvanced.ThrowIfNull(
                declaringType,
                nameof(declaringTypeHandle),
                $"Invalid argument '{nameof(declaringTypeHandle)}'. The provided declaring type handle does not resolve to a runtime type.");

            DeclaringTypeHandle = declaringTypeHandle;
            ConstructorParameterList = constructorParameters.OrEmpty();
            IsAnonymous = true;
        }

        public RuntimeTypeHandle DeclaringTypeHandle { get; }
        public MethodParameterInfoList ConstructorParameterList { get; }
        public bool IsAnonymous { get; }

        public bool Equals(AnonymousConstructorDescriptor other)
            => DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && ConstructorParameterList.Equals(other.ConstructorParameterList)
            && IsAnonymous == other.IsAnonymous;

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(DeclaringTypeHandle);
            hashCode.Add(ConstructorParameterList);
            hashCode.Add(IsAnonymous);
            hashCode.Add(DeclaringTypeHandle);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(AnonymousConstructorDescriptor left, AnonymousConstructorDescriptor right)
            => left.Equals(right);
        public static bool operator !=(AnonymousConstructorDescriptor left, AnonymousConstructorDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is AnonymousConstructorDescriptor other && Equals(other);
    }
}
