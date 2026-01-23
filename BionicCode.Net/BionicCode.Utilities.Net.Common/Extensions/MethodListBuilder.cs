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

    internal static class MethodListBuilder : SymbolDataListBuilder<PropertyData>, IPropertyListBuilder
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
        internal static MethodList Create(IEnumerable<MethodInfo>? items)
        {
            List<MethodInfo>? methodInfoList = items?.ToList();
            if (methodInfoList is null || methodInfoList.IsEmpty())
            {
                return MethodList.Empty;
            }

            List<MethodData> methods = new List<MethodData>(methodInfoList.Count);
            RuntimeTypeHandle declaringTypeHandle = default;
            foreach (MethodInfo methodInfo in methodInfoList)
            {
                MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);

                if (declaringTypeHandle.Equals(default))
                {
                    declaringTypeHandle = methodData.DeclaringTypeHandle;
                }

                if (!methodData.DeclaringTypeHandle.Equals(declaringTypeHandle))
                {
                    throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(MethodInfo)}' items must belong to the same declaring type.");
                }

                methods.Add(methodData);
            }

            return methods.ToMethodList();
        }

        internal static MethodList Create(TypeData declaringTypeData)
        {
            ArgumentNullException.ThrowIfNull(declaringTypeData);
            return CreateInternal(declaringTypeData.UnwrapType());
        }

        internal static MethodList Create(Type declaringType)
        {
            ArgumentNullException.ThrowIfNull(declaringType);
            return CreateInternal(declaringType);
        }

        private static MethodList CreateInternal(Type declaringType)
        {
            MethodInfo[] methodInfoList = declaringType.GetMethods(HelperExtensionsCommon.AllMembersFullHierarchyFlags);
            if (methodInfoList.IsEmpty())
            {
                return MethodList.Empty;
            }

            IEnumerable<MethodData> methods = methodInfoList.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry);

            return methods.ToMethodList();
        }

        IPropertyListBuilder IPropertyListBuilder.Add(PropertyData propertyData)
        {
            Add(propertyData);
            return this;
        }

        PropertyList IPropertyListBuilder.Build()
            => this._builderResult ??= new PropertyList(Build(), isIntegrityValidationEnabled: false);

        internal static MethodList ToMethodList(this IEnumerable<MethodData> items)
            => items is null || items.IsEmpty() ? MethodList.Empty : new MethodList(items);
    }

    internal static class PropertyListBuilderExtensions
    {
        public static PropertyList ToPropertyList(this IEnumerable<PropertyData> items)
            => items is null || items.IsEmpty() ? PropertyList.Empty : new PropertyList(items);

        public static PropertyList OrEmpty(this PropertyList items)
            => items ?? PropertyList.Empty;
    }
}
