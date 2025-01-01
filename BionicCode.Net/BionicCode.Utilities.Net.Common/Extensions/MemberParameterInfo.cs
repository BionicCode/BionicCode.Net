namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.InteropServices;

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

    public static MemberParameterInfo[] ConvertFrom(Type[] parameterList)
      => parameterList.Select(parameterType => new MemberParameterInfo(parameterType, parameterType.IsGenericParameter))
        .ToArray();

    public static MemberParameterInfo[] ConvertFrom(TypeData[] parameterList)
      => parameterList.Select(parameterData => parameterData.GetType())
        .Select(parameterType => new MemberParameterInfo(parameterType, parameterType.IsGenericParameter))
        .ToArray();

    public static MemberParameterInfo[] ConvertFrom(ParameterInfo[] parameterList)
      => parameterList.Select(parameterInfo => parameterInfo.ParameterType)
        .Select(parameterType => new MemberParameterInfo(parameterType, parameterType.IsGenericParameter))
        .ToArray();

    public static MemberParameterInfo[] ConvertFrom(ParameterData[] parameterList)
      => parameterList.Select(parameterData => parameterData.ParameterTypeData.GetType())
        .Select(parameterType => new MemberParameterInfo(parameterType, parameterType.IsGenericParameter))
        .ToArray();

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