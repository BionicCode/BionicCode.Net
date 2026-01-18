namespace BionicCode.Utilities.Net
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    internal readonly struct MethodDataGenericTypeVariantKey : IEquatable<MethodDataGenericTypeVariantKey>
    {
        public MethodDataGenericTypeVariantKey(TypeList genericMethodArguments, RuntimeTypeHandle desiredReturnTypeHandle, RuntimeTypeHandle targetTypeHandle, BasicMethodFingerprint basicMethodFingerprint)
        {
            ArgumentNullException.ThrowIfNull(genericMethodArguments);
            ArgumentNullExceptionAdvanced.ThrowIfDefault(desiredReturnTypeHandle);
            ArgumentNullExceptionAdvanced.ThrowIfDefault(targetTypeHandle);
            ArgumentNullExceptionAdvanced.ThrowIfDefault(basicMethodFingerprint);

            this.GenericMethodArguments = genericMethodArguments;
            this.DesiredReturnTypeHandle = desiredReturnTypeHandle;
            this.TargetTypeHandle = targetTypeHandle;
            this.BasicMethodFingerprint = basicMethodFingerprint;

            this._hashCode = ComputeHashCode();
        }

        public TypeList GenericMethodArguments { get; }
        public BasicMethodFingerprint BasicMethodFingerprint { get; }
        public RuntimeTypeHandle DesiredReturnTypeHandle { get; }
        public RuntimeTypeHandle TargetTypeHandle { get; }
        private readonly int _hashCode;

        public bool Equals(MethodDataGenericTypeVariantKey other) => this.DesiredReturnTypeHandle.Equals(other.DesiredReturnTypeHandle)
            && this.TargetTypeHandle.Equals(other.TargetTypeHandle)
            && this.BasicMethodFingerprint == other.BasicMethodFingerprint
            && this.GenericMethodArguments.Equals(other.GenericMethodArguments);

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is MethodDataGenericTypeVariantKey other && Equals(other);

        public override int GetHashCode()
            => this._hashCode;

        private int ComputeHashCode()
        {
            return HashCode.Combine(
                this.GenericMethodArguments,
                this.DesiredReturnTypeHandle,
                this.TargetTypeHandle,
                this.BasicMethodFingerprint);
        }

        public static bool operator ==(MethodDataGenericTypeVariantKey left, MethodDataGenericTypeVariantKey right) => left.Equals(right);
        public static bool operator !=(MethodDataGenericTypeVariantKey left, MethodDataGenericTypeVariantKey right) => !(left == right);
    }
}
