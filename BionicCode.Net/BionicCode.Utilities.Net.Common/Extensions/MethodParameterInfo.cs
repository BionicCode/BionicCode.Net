namespace BionicCode.Utilities.Net
{
    using System;

    internal readonly struct MethodParameterInfo : IEquatable<MethodParameterInfo>
    {
        public int Position { get; }
        public RuntimeTypeHandle ParameterTypeHandle { get; }
        public RuntimeTypeHandle DeclaringTypeHandle { get; }
        public ParameterKind Kind { get; }
        public bool IsGenericMethodParameter { get; }

        public MethodParameterInfo(Type type, int position, bool isGenericMethodParameter, ParameterKind kind, Type declaringType) : this()
        {
            this.Position = position;
            this.IsGenericMethodParameter = isGenericMethodParameter;
            this.ParameterTypeHandle = type.TypeHandle;
            this.Kind = kind;
            this.DeclaringTypeHandle = declaringType.TypeHandle;
        }

        public override bool Equals(object obj) => obj is MethodParameterInfo info && Equals(info);
        public bool Equals(MethodParameterInfo other) => other.ParameterTypeHandle.Equals(this.ParameterTypeHandle)
            && other.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle)
            && other.Position == this.Position
            && other.Kind.Equals(this.Kind)
            && other.IsGenericMethodParameter.Equals(this.IsGenericMethodParameter);

        public override int GetHashCode()
            => HashCode.Combine(this.ParameterTypeHandle, this.DeclaringTypeHandle, this.Position, this.Kind, this.IsGenericMethodParameter);
    }
}
