namespace BionicCode.Utilities.Net.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal interface IParameterListBuilder
    {
        IParameterListBuilder Add(ParameterData parameterData);
        ParameterList Build();
    }

    internal class ParameterListBuilder : SymbolDataListBuilder<ParameterData>, IParameterListBuilder
    {
        private ParameterList? _builderResult;

        private ParameterListBuilder(RuntimeTypeHandle declaringTypeHandle) : base(declaringTypeHandle)
        {
        }

        public static IParameterListBuilder New(RuntimeTypeHandle declaringTypeHandle)
        {
            var builder = new ParameterListBuilder(declaringTypeHandle);
            return builder;
        }
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
            ParameterList parameters = ParameterList.Empty;
            switch (propertyParameterSource)
            {
                case PropertyParameterSource.PropertySetMethod:
                    {
                        if (!propertyData.CanWrite)
                        {
                            return parameters;
                        }

                        parameters = propertyData.PropertySetMethodParameters;

                        break;
                    }
                case PropertyParameterSource.PropertyGetMethod:
                    {
                        if (!propertyData.CanRead)
                        {
                            return parameters;
                        }

                        parameters = propertyData.PropertyGetMethodParameters;

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

        IParameterListBuilder IParameterListBuilder.Add(ParameterData propertyData)
        {
            Add(propertyData);
            return this;
        }

        ParameterList IParameterListBuilder.Build()
            => this._builderResult ??= new ParameterList(Build(), isIntegrityValidationEnabled: false);

        private enum PropertyParameterSource
        {
            Undefined,
            PropertySetMethod,
            PropertyGetMethod,
        }
    }

    internal static class ParameterListBuilderExtensions
    {
        internal static ParameterList ToParameterList(this IEnumerable<ParameterData>? items)
            => items is null || items.IsEmpty() ? ParameterList.Empty : new ParameterList(items);

        /// <summary>
        /// Returns an empty <see cref="ParameterList"/> if the provided instance is <see langword="null"/>.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>A <see cref="ParameterList"/> that is empty if the provided instance is <see langword="null"/>. Otherwise, returns the original instance.</returns>
        internal static ParameterList OrEmpty(this ParameterList items)
            => items ?? ParameterList.Empty;
    }
}
