namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides information about a well-known field.
    /// </summary>
    /// <remarks>The <see cref="WellKnownFieldDescriptor"/> is used to provide information for well-known field symbols, which is when the caller has the direct <see cref="FieldInfo"/> representation.
    /// <para/>When the caller does not have the direct <see cref="FieldInfo"/> representation and only signature information is available the field symbol is considered anonymous. In such case use the <see cref="AnonymousFieldDescriptor"/> instead.
    /// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="FieldInfo"/> is available to ensure maximum accuracy and performance.
    /// </remarks>
    internal readonly struct WellKnownFieldDescriptor : IEquatable<WellKnownFieldDescriptor>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="WellKnownFieldDescriptor"/> struct for a well-known field.
        /// </summary>
        /// <remarks>
        /// For best accuracy and performance always use this <see cref="WellKnownFieldDescriptor"/> when the caller has direct access to the <see cref="FieldInfo"/> representation of the field.
        /// </remarks>
        /// <param name="fieldInfo">The <see cref="FieldInfo"/> that the descriptor represents.</param>
        /// <returns>A new instance of <see cref="WellKnownFieldDescriptor"/> representing the specified well-known field.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
        public WellKnownFieldDescriptor(FieldInfo fieldInfo)
        {
            ArgumentNullException.ThrowIfNull(fieldInfo);

            this.FieldHandle = fieldInfo.FieldHandle;
            this.FieldName = fieldInfo.Name;
        }

        public RuntimeFieldHandle FieldHandle { get; }
        public string FieldName { get; }
        public bool IsAnonymous { get; }

        public bool Equals(WellKnownFieldDescriptor other)
            => this.FieldHandle.Equals(other.FieldHandle)
            && this.FieldName.Equals(other.FieldName, StringComparison.Ordinal)
            && this.IsAnonymous == other.IsAnonymous;

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.IsAnonymous);
            hashCode.Add(this.FieldHandle);
            hashCode.Add(this.FieldName, StringComparer.Ordinal);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(WellKnownFieldDescriptor left, WellKnownFieldDescriptor right)
            => left.Equals(right);
        public static bool operator !=(WellKnownFieldDescriptor left, WellKnownFieldDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is WellKnownFieldDescriptor other && Equals(other);
    }
}
