namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal interface IMethodParameterInfoListBuilder
    {
        IMethodParameterInfoListBuilder Add(MethodParameterInfo methodParameterInfo);
        MethodParameterInfoList Build();
    }

    internal class MethodParameterInfoListBuilder : SymbolDataListBuilder<MethodParameterInfo>, IMethodParameterInfoListBuilder
    {
        private MethodParameterInfoList? _builderResult;

        private MethodParameterInfoListBuilder(RuntimeTypeHandle declaringTypeHandle) : base(declaringTypeHandle)
        {
        }

        public static IMethodParameterInfoListBuilder New(RuntimeTypeHandle declaringTypeHandle)
        {
            var builder = new MethodParameterInfoListBuilder(declaringTypeHandle);
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

        IMethodParameterInfoListBuilder IMethodParameterInfoListBuilder.Add(MethodParameterInfo methodParameterInfo)
        {
            Add(methodParameterInfo);
            return this;
        }

        MethodParameterInfoList IMethodParameterInfoListBuilder.Build()
            => this._builderResult ??= new MethodParameterInfoList(Build(), isIntegrityValidationEnabled: false);
    }

    internal static class MethodParameterInfoListBuilderExtensions
    {
        public static MethodParameterInfoList AsMethodParameterInfoList(this IEnumerable<ParameterData> items)
            => items is null || items.IsEmpty() ? MethodParameterInfoList.Empty : new MethodParameterInfoList(items.Select(parameterData => new MethodParameterInfo(parameterData)));

        public static MethodParameterInfoList OrEmpty(this MethodParameterInfoList items)
            => items ?? MethodParameterInfoList.Empty;
    }
}
