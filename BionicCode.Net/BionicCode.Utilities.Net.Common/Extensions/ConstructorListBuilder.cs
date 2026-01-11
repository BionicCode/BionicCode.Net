namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class ConstructorListBuilder
    {
        internal static ConstructorList Create(IEnumerable<ConstructorInfo>? items)
        {
            List<ConstructorInfo>? constructorInfoList = items?.ToList();
            if (constructorInfoList is null || constructorInfoList.IsEmpty())
            {
                return ConstructorList.Empty;
            }

            List<ConstructorData> constructors = new List<ConstructorData>(constructorInfoList.Count);
            RuntimeTypeHandle declaringTypeHandle = default;
            foreach (ConstructorInfo constructorInfo in constructorInfoList)
            {
                ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);

                if (declaringTypeHandle.Equals(default))
                {
                    declaringTypeHandle = constructorData.DeclaringTypeHandle;
                }

                if (!constructorData.DeclaringTypeHandle.Equals(declaringTypeHandle))
                {
                    throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(ConstructorInfo)}' items must belong to the same declaring type.");
                }

                constructors.Add(constructorData);
            }

            return constructors.ToConstructorList();
        }

        internal static ConstructorList Create(TypeData declaringTypeData)
        {
            ArgumentNullException.ThrowIfNull(declaringTypeData);
            return CreateInternal(declaringTypeData.UnwrapType());
        }

        internal static ConstructorList Create(Type declaringType)
        {
            ArgumentNullException.ThrowIfNull(declaringType);
            return CreateInternal(declaringType);
        }

        private static ConstructorList CreateInternal(Type declaringType)
        {
            ConstructorInfo[] constructorInfoList = declaringType.GetConstructors(HelperExtensionsCommon.AllMembersFullHierarchyFlags);
            if (constructorInfoList.IsEmpty())
            {
                return ConstructorList.Empty;
            }

            IEnumerable<ConstructorData> constructors = constructorInfoList.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry);

            return constructors.ToConstructorList();
        }

        internal static ConstructorList ToConstructorList(this IEnumerable<ConstructorData> items)
            => items is null || items.IsEmpty() ? ConstructorList.Empty : new ConstructorList(items);
    }
}
