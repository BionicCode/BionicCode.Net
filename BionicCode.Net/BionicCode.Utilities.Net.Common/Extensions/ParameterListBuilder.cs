namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class ParameterListBuilder
    {
        internal static ParameterList Create(IEnumerable<ParameterInfo>? items)
        {
            List<ParameterInfo>? parameterInfoList = items?.ToList();
            if (parameterInfoList is null || parameterInfoList.IsEmpty())
            {
                return ParameterList.Empty;
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

            return parameters.ToParameterList();
        }

        internal static ParameterList Create(PropertyData propertyData)
        {
            ArgumentNullException.ThrowIfNull(propertyData);
            return CreateInternal(propertyData.GetPropertyInfo());
        }

        internal static ParameterList Create(PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);
            return CreateInternal(propertyInfo);
        }

        private static ParameterList CreateInternal(PropertyInfo propertyInfo)
        {
            ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
            if (indexParameters.IsEmpty())
            {
                return ParameterList.Empty;
            }

            IEnumerable<ParameterData> parameters = indexParameters.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry);
            return parameters.ToParameterList();
        }

        internal static ParameterList Create(MethodData methodData)
            => Create(methodData.GetMethodInfo());

        internal static ParameterList Create(ConstructorData constructorData)
            => Create(constructorData.GetConstructorInfo());

        internal static ParameterList Create(MethodBase methodBase)
        {
            ArgumentNullException.ThrowIfNull(methodBase);

            ParameterInfo[] parameterInfoList = methodBase.GetParameters();
            if (parameterInfoList.IsEmpty())
            {
                return ParameterList.Empty;
            }

            IEnumerable<ParameterData> parameters = parameterInfoList.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry);

            return parameters.ToParameterList();
        }

        internal static ParameterList ToParameterList(this IEnumerable<ParameterData>? items)
            => items is null || items.IsEmpty() ? ParameterList.Empty : new ParameterList(items);
    }
}
