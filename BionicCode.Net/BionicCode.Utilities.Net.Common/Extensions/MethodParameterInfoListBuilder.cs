namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class MethodParameterInfoListBuilder
    {
        internal static MethodParameterInfoList Create(IEnumerable<ParameterData> items)
        {
            List<ParameterData>? parameterDataList = items?.ToList();
            if (parameterDataList is null || parameterDataList.IsEmpty())
            {
                return MethodParameterInfoList.Empty;
            }

            List<ParameterData> parameters = new List<ParameterData>(parameterDataList.Count);
            SymbolInfoData? member = null;
            foreach (ParameterData parameterData in parameterDataList)
            {
                if (member == null)
                {
                    member = parameterData.MemberData;
                }

                if (!ReferenceEquals(parameterData.MemberData, member))
                {
                    throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(MethodParameterInfo)}' items must belong to the same member.");
                }

                parameters.Add(parameterData);
            }

            return parameters.AsMethodParameterInfoList();
        }

        internal static MethodParameterInfoList Create(IEnumerable<ParameterInfo> items)
        {
            List<ParameterInfo>? parameterInfoList = items?.ToList();
            if (parameterInfoList is null || parameterInfoList.IsEmpty())
            {
                return MethodParameterInfoList.Empty;
            }

            List<ParameterData> parameters = new List<ParameterData>(parameterInfoList.Count);
            SymbolInfoData? member = null;
            foreach (ParameterInfo parameterInfo in parameterInfoList)
            {
                ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);

                if (member == null)
                {
                    member = parameterData.MemberData;
                }

                if (!ReferenceEquals(parameterData.MemberData, member))
                {
                    throw new ArgumentException($"All '{nameof(ParameterInfo)}' items must belong to the same member.");
                }

                parameters.Add(parameterData);
            }

            return parameters.AsMethodParameterInfoList();
        }

        internal static MethodParameterInfoList Create(PropertyData propertyData)
        {
            ArgumentNullException.ThrowIfNull(propertyData);

            return CreateInternal(propertyData.GetPropertyInfo());
        }

        internal static MethodParameterInfoList Create(PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);

            return CreateInternal(propertyInfo);
        }

        internal static MethodParameterInfoList Create(MethodData methodData)
        {
            ArgumentNullException.ThrowIfNull(methodData);

            return CreateInternal(methodData.GetMethodInfo());
        }

        internal static MethodParameterInfoList Create(ConstructorData constructorData)
        {
            ArgumentNullException.ThrowIfNull(constructorData);

            return CreateInternal(constructorData.GetConstructorInfo());
        }

        internal static MethodParameterInfoList Create(MethodBase methodBase)
        {
            ArgumentNullException.ThrowIfNull(methodBase);

            return CreateInternal(methodBase);
        }

        private static MethodParameterInfoList CreateInternal(MethodBase methodBase)
        {
            ParameterInfo[] parameterInfoList = methodBase.GetParameters();
            if (parameterInfoList.IsEmpty())
            {
                return MethodParameterInfoList.Empty;
            }

            IEnumerable<ParameterData> parameters = parameterInfoList.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry);

            return parameters.AsMethodParameterInfoList();
        }

        private static MethodParameterInfoList CreateInternal(PropertyInfo propertyInfo)
        {
            ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
            if (indexParameters.IsEmpty())
            {
                return MethodParameterInfoList.Empty;
            }

            IEnumerable<ParameterData> parameters = indexParameters.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry);
            return parameters.AsMethodParameterInfoList();
        }

        internal static MethodParameterInfoList AsMethodParameterInfoList(this IEnumerable<ParameterData>? items)
            => items is null || items.IsEmpty() ? MethodParameterInfoList.Empty : new MethodParameterInfoList(items.Select(parameterData => new MethodParameterInfo(
                parameterData.ParameterTypeHandle,
                parameterData.Position,
                parameterData.IsGenericMethodParameter,
                parameterData.ParameterKind,
                parameterData.DeclaringTypeHandle)));
    }
}
