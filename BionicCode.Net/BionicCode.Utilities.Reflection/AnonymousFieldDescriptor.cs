namespace BionicCode.Utilities.Net.Reflection
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides information about an anonymous field.
    /// </summary>
    /// <remarks>The <see cref="AnonymousFieldDescriptor"/> is used to provide information for anonymous field symbols, which is when the caller does not have a <see cref="FieldInfo"/> representation but only signature information.
    /// <para/>When the caller does have the direct <see cref="FieldInfo"/> representation, the field symbol is considered well-known. In such case use the <see cref="WellKnownFieldDescriptor"/> instead.
    /// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="FieldInfo"/> is available to ensure maximum accuracy and performance.
    /// </remarks>
    internal readonly struct AnonymousFieldDescriptor : IEquatable<AnonymousFieldDescriptor>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AnonymousFieldDescriptor"/> struct for an anonymous field.
        /// </summary>
        /// <remarks>
        /// For best accuracy and performance always use the <see cref="WellKnownFieldDescriptor"/> when the caller has direct access to the <see cref="FieldInfo"/> representation of the field.
        /// </remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous field.</param>
        /// <param name="fieldName">The name of the anonymous field.</param>
        /// <returns>A new instance of <see cref="AnonymousFieldDescriptor"/> representing the specified anonymous field.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldName"/> is <see langword="null"/> or whitespace.</exception>
        public AnonymousFieldDescriptor(RuntimeTypeHandle declaringTypeHandle, string fieldName)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fieldName);

            this.DeclaringTypeHandle = declaringTypeHandle;
            this.FieldName = fieldName;
            this.IsAnonymous = true;
        }

        public RuntimeTypeHandle DeclaringTypeHandle { get; }
        public string FieldName { get; }
        public bool IsAnonymous { get; }

        public bool Equals(AnonymousFieldDescriptor other)
            => this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && this.FieldName == other.FieldName
            && this.IsAnonymous == other.IsAnonymous;

        public override int GetHashCode()
        {
            var hasCode = new HashCode();
            hasCode.Add(this.IsAnonymous);
            hasCode.Add(this.DeclaringTypeHandle);
            hasCode.Add(this.FieldName);

            return hasCode.ToHashCode();
        }

        public static bool operator ==(AnonymousFieldDescriptor left, AnonymousFieldDescriptor right)
            => left.Equals(right);
        public static bool operator !=(AnonymousFieldDescriptor left, AnonymousFieldDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is AnonymousFieldDescriptor other && Equals(other);
    }
}
