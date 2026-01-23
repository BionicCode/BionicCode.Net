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

    internal class PropertyListBuilder : SymbolDataListBuilder<PropertyData>, IPropertyListBuilder
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

        public static PropertyList Create(IEnumerable<PropertyInfo>? items)
        {
            List<PropertyInfo>? propertyInfoList = items?.ToList();
            if (propertyInfoList is null || propertyInfoList.IsEmpty())
            {
                return PropertyList.Empty;
            }

            List<PropertyData> properties = new List<PropertyData>(propertyInfoList.Count);
            RuntimeTypeHandle declaringTypeHandle = default;
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);

                if (declaringTypeHandle.Equals(default))
                {
                    declaringTypeHandle = propertyData.DeclaringTypeHandle;
                }

                if (!propertyData.DeclaringTypeHandle.Equals(declaringTypeHandle))
                {
                    throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(PropertyInfo)}' items must belong to the same declaring type.");
                }

                properties.Add(propertyData);
            }

            return properties.ToPropertyList();
        }

        public static PropertyList Create(TypeData declaringTypeData)
        {
            ArgumentNullException.ThrowIfNull(declaringTypeData);
            return CreateInternal(declaringTypeData.UnwrapType());
        }

        public static PropertyList Create(Type declaringType)
        {
            ArgumentNullException.ThrowIfNull(declaringType);
            return CreateInternal(declaringType);
        }

        public static PropertyList CreateInternal(Type declaringType)
        {
            PropertyInfo[] propertyInfoList = declaringType.GetProperties(HelperExtensionsCommon.AllMembersFullHierarchyFlags);
            if (propertyInfoList.IsEmpty())
            {
                return PropertyList.Empty;
            }

            IEnumerable<PropertyData> properties = propertyInfoList.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry);

            return properties.ToPropertyList();
        }

        IPropertyListBuilder IPropertyListBuilder.Add(PropertyData propertyData)
        {
            Add(propertyData);
            return this;
        }

        PropertyList IPropertyListBuilder.Build()
            => this._builderResult ??= new PropertyList(Build(), isIntegrityValidationEnabled: false);
    }

    internal static class PropertyListBuilderExtensions
    {
        public static PropertyList ToPropertyList(this IEnumerable<PropertyData> items)
            => items is null || items.IsEmpty() ? PropertyList.Empty : new PropertyList(items);

        public static PropertyList OrEmpty(this PropertyList items)
            => items ?? PropertyList.Empty;
    }
}
