namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal interface IPropertyListBuilder
    {
        IPropertyListBuilder Add(PropertyData propertyData);
        PropertyList Build();
    }

    internal static class MethodParameterInfoListBuilder : SymbolDataListBuilder<PropertyData>, IPropertyListBuilder
    {
        private PropertyList? _builderResult;

        private PropertyListBuilder(RuntimeTypeHandle declaringTypeHandle) : base(declaringTypeHandle)
        {
        }

        public static IPropertyListBuilder New(RuntimeTypeHandle declaringTypeHandle)
        {
            var builder = new PropertyListBuilder(declaringTypeHandle);
            return builder;
        }
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
        internal static MethodParameterInfoList Create(ReadOnlySpan<ParameterData> items)
        {
            if (items.IsEmpty)
            {
                return MethodParameterInfoList.Empty;
            }

            List<ParameterData> parameters = new List<ParameterData>(items.Length);
            SymbolInfoData? member = null;
            foreach (ParameterData parameterData in items)
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

        internal static MethodParameterInfoList Create(IEnumerable<MethodParameterInfo> items)
        {
            List<MethodParameterInfo>? parameterInfoList = items?.ToList();
            return parameterInfoList is null || parameterInfoList.IsEmpty()
                ? MethodParameterInfoList.Empty
                : new MethodParameterInfoList(parameterInfoList);
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

        IPropertyListBuilder IPropertyListBuilder.Add(PropertyData propertyData)
        {
            Add(propertyData);
            return this;
        }

        PropertyList IPropertyListBuilder.Build()
            => this._builderResult ??= new PropertyList(Build(), isIntegrityValidationEnabled: false);

        internal static MethodParameterInfoList AsMethodParameterInfoList(this IEnumerable<ParameterData>? items)
            => items is null || items.IsEmpty() ? MethodParameterInfoList.Empty : new MethodParameterInfoList(items.Select(parameterData => new MethodParameterInfo(parameterData)));
    }

    internal static class PropertyListBuilderExtensions
    {
        public static PropertyList ToPropertyList(this IEnumerable<PropertyData> items)
            => items is null || items.IsEmpty() ? PropertyList.Empty : new PropertyList(items);

        public static PropertyList OrEmpty(this PropertyList items)
            => items ?? PropertyList.Empty;
    }
}
