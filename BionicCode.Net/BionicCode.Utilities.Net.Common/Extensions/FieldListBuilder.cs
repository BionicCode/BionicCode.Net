namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class FieldListBuilder
    {
        internal static FieldList Create(IEnumerable<FieldInfo>? items)
        {
            List<FieldInfo>? fieldInfoList = items?.ToList();
            if (fieldInfoList is null || fieldInfoList.IsEmpty())
            {
                return FieldList.Empty;
            }

            List<FieldData> fields = new List<FieldData>(fieldInfoList.Count);
            RuntimeTypeHandle declaringTypeHandle = default;
            foreach (FieldInfo fieldInfo in fieldInfoList)
            {
                FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);

                if (declaringTypeHandle.Equals(default))
                {
                    declaringTypeHandle = fieldData.DeclaringTypeHandle;
                }

                if (!fieldData.DeclaringTypeHandle.Equals(declaringTypeHandle))
                {
                    throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(FieldInfo)}' items must belong to the same declaring type.");
                }

                fields.Add(fieldData);
            }

            return fields.ToFieldList();
        }

        internal static FieldList Create(TypeData declaringTypeData)
        {
            ArgumentNullException.ThrowIfNull(declaringTypeData);
            return CreateInternal(declaringTypeData.UnwrapType());
        }

        internal static FieldList Create(Type declaringType)
        {
            ArgumentNullException.ThrowIfNull(declaringType);
            return CreateInternal(declaringType);
        }

        private static FieldList CreateInternal(Type declaringType)
        {
            FieldInfo[] fieldInfoList = declaringType.GetFields(HelperExtensionsCommon.AllMembersFullHierarchyFlags);
            if (fieldInfoList.IsEmpty())
            {
                return FieldList.Empty;
            }

            IEnumerable<FieldData> fields = fieldInfoList.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry);

            return fields.ToFieldList();
        }

        internal static FieldList ToFieldList(this IEnumerable<FieldData> items)
            => items is null || items.IsEmpty() ? FieldList.Empty : new FieldList(items);
    }
}
