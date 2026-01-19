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

        internal static ParameterList CreateForIndexer(PropertyData propertyData)
        {
            ArgumentNullException.ThrowIfNull(propertyData);
            return CreateInternal(propertyData, PropertyParameterSource.Indexer);
        }

        internal static ParameterList CreateForIndexer(PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);
            return CreateInternal(propertyInfo.ToPropertyData(), PropertyParameterSource.Indexer);
        }

        internal static ParameterList CreateForPropertyGet(PropertyData propertyData)
        {
            ArgumentNullException.ThrowIfNull(propertyData);
            return CreateInternal(propertyData, PropertyParameterSource.PropertyGetMethod);
        }

        internal static ParameterList CreateForPropertyGet(PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);
            return CreateInternal(propertyInfo.ToPropertyData(), PropertyParameterSource.PropertyGetMethod);
        }

        internal static ParameterList CreateForPropertySet(PropertyData propertyData)
        {
            ArgumentNullException.ThrowIfNull(propertyData);
            return CreateInternal(propertyData, PropertyParameterSource.PropertySetMethod);
        }

        internal static ParameterList CreateForPropertySet(PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);
            return CreateInternal(propertyInfo.ToPropertyData(), PropertyParameterSource.PropertySetMethod);
        }

        private static ParameterList CreateInternal(PropertyData propertyData, PropertyParameterSource propertyParameterSource)
        {
            // Use PropertyInfo for parameter related operations to avoid circular references in PropertyData
            PropertyInfo propertyInfo = propertyData.GetPropertyInfo();

            ParameterList parameters = ParameterList.Empty;
            switch (propertyParameterSource)
            {
                case PropertyParameterSource.Indexer:
                    {
                        ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
                        if (indexParameters.Length == 0)
                        {
                            return parameters;
                        }

                        parameters = propertyInfo.GetIndexParameters()
                            .Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry)
                            .ToParameterList();

                        break;
                    }
                case PropertyParameterSource.PropertySetMethod:
                    {
                        if (!propertyData.CanWrite)
                        {
                            return parameters;
                        }

                        parameters = propertyData.PropertySetMethodData.Parameters;

                        break;
                    }
                case PropertyParameterSource.PropertyGetMethod:
                    {
                        if (!propertyData.CanRead)
                        {
                            return parameters;
                        }

                        parameters = propertyData.PropertyGetMethodData.Parameters;

                        break;
                    }
            }

            return parameters;
        }

        internal static ParameterList Create(ParameterizedMemberData parameterizedMember)
            => Create(parameterizedMember.GetMethodBase());

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

        internal static ParameterList OrEmpty(this ParameterList items)
            => items ?? ParameterList.Empty;

        private enum PropertyParameterSource
        {
            Undefined,
            Indexer,
            PropertySetMethod,
            PropertyGetMethod,
        }
    }
}
