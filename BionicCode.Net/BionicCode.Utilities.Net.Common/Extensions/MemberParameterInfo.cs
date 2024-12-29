namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;

  internal readonly struct MemberParameterInfo : IEquatable<MemberParameterInfo>
  {
    public MemberParameterInfo(Type parameterType, bool isGenericTypeParameter)
    {
      this.ParameterType = parameterType;
      this.IsGenericTypeParameter = isGenericTypeParameter;
    }

    public MemberParameterInfo(TypeData parameterType, bool isGenericTypeParameter)
    {
      this.ParameterType = parameterType.GetType();
      this.IsGenericTypeParameter = isGenericTypeParameter;
    }

    public MemberParameterInfo(ParameterData parameterData, bool isGenericTypeParameter)
    {
      this.ParameterType = parameterData.ParameterTypeData.GetType();
      this.IsGenericTypeParameter = isGenericTypeParameter;
    }

    public Type ParameterType { get; }
    public bool IsGenericTypeParameter { get; }

    public override bool Equals(object obj) => obj is MemberParameterInfo info && Equals(info);
    public bool Equals(MemberParameterInfo other) => other.ParameterType.Equals(this.ParameterType) && other.IsGenericTypeParameter == this.IsGenericTypeParameter;

    public override int GetHashCode()
    {
      int hashCode = 587076725;
      hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(this.ParameterType);
      hashCode = hashCode * -1521134295 + this.IsGenericTypeParameter.GetHashCode();
      return hashCode;
    }
  }
}