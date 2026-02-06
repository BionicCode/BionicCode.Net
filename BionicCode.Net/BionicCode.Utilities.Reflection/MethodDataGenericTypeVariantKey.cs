namespace BionicCode.Utilities.Net.Reflection
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

            GenericMethodArguments = genericMethodArguments;
            DesiredReturnTypeHandle = desiredReturnTypeHandle;
            TargetTypeHandle = targetTypeHandle;
            BasicMethodFingerprint = basicMethodFingerprint;

            _hashCode = ComputeHashCode();
        }

        public TypeList GenericMethodArguments { get; }
        public BasicMethodFingerprint BasicMethodFingerprint { get; }
        public RuntimeTypeHandle DesiredReturnTypeHandle { get; }
        public RuntimeTypeHandle TargetTypeHandle { get; }
        private readonly int _hashCode;

        public bool Equals(MethodDataGenericTypeVariantKey other) => DesiredReturnTypeHandle.Equals(other.DesiredReturnTypeHandle)
            && TargetTypeHandle.Equals(other.TargetTypeHandle)
            && BasicMethodFingerprint == other.BasicMethodFingerprint
            && GenericMethodArguments.Equals(other.GenericMethodArguments);

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is MethodDataGenericTypeVariantKey other && Equals(other);

        public override int GetHashCode()
            => _hashCode;

        private int ComputeHashCode()
        {
            return HashCode.Combine(
                GenericMethodArguments,
                DesiredReturnTypeHandle,
                TargetTypeHandle,
                BasicMethodFingerprint);
        }

        public static bool operator ==(MethodDataGenericTypeVariantKey left, MethodDataGenericTypeVariantKey right) => left.Equals(right);
        public static bool operator !=(MethodDataGenericTypeVariantKey left, MethodDataGenericTypeVariantKey right) => !(left == right);
    }
}
