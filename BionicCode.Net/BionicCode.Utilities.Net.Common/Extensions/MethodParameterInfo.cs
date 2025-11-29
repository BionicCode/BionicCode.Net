namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal readonly struct MethodParameterInfo : IEquatable<MethodParameterInfo>
    {
        public MethodParameterInfo(Type parameterType, bool isGenericTypeParameter)
        {
            this.ParameterType = parameterType;
            this.IsGenericTypeParameter = isGenericTypeParameter;
        }

        public MethodParameterInfo(TypeData parameterType, bool isGenericTypeParameter)
        {
            this.ParameterType = parameterType.GetType();
            this.IsGenericTypeParameter = isGenericTypeParameter;
        }

        public MethodParameterInfo(ParameterData parameterData, bool isGenericTypeParameter)
        {
            this.ParameterType = parameterData.ParameterTypeData.GetType();
            this.IsGenericTypeParameter = isGenericTypeParameter;
        }

        public static ParameterList ConvertFrom(Type[] parameterList)
          => new ParameterList(parameterList.Select(parameterType => new MethodParameterInfo(parameterType, parameterType.IsGenericParameter))
            .ToArray());

        public static MethodParameterInfo[] ConvertFrom(TypeData[] parameterList)
          => parameterList.Select(parameterData => parameterData.GetType())
            .Select(parameterType => new MethodParameterInfo(parameterType, parameterType.IsGenericParameter))
            .ToArray();

        public static MethodParameterInfo[] ConvertFrom(ParameterInfo[] parameterList)
          => parameterList.Select(parameterInfo => parameterInfo.ParameterType)
            .Select(parameterType => new MethodParameterInfo(parameterType, parameterType.IsGenericParameter))
            .ToArray();

        public static MethodParameterInfo[] ConvertFrom(ParameterData[] parameterList)
          => parameterList.Select(parameterData => parameterData.ParameterTypeData.GetType())
            .Select(parameterType => new MethodParameterInfo(parameterType, parameterType.IsGenericParameter))
            .ToArray();

        public Type ParameterType { get; }
        public Type ParameterIndex { get; }
        public Type ParameterName { get; }
        public bool IsGenericTypeParameter { get; }

        public override bool Equals(object obj) => obj is MethodParameterInfo info && Equals(info);
        public bool Equals(MethodParameterInfo other) => other.ParameterType.Equals(this.ParameterType) && other.IsGenericTypeParameter == this.IsGenericTypeParameter;

        public override int GetHashCode()
        {
            int hashCode = 587076725;
            hashCode = (hashCode * -1521134295) + EqualityComparer<Type>.Default.GetHashCode(this.ParameterType);
            hashCode = (hashCode * -1521134295) + this.IsGenericTypeParameter.GetHashCode();
            return hashCode;
        }
    }
}
