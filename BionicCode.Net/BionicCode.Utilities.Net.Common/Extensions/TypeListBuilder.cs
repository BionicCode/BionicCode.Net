namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class TypeListBuilder
    {
        internal static TypeList Create(IEnumerable<Type> items)
        {
            List<Type>? types = items?.ToList();
            if (types is null || types.IsEmpty())
            {
                return TypeList.Empty;
            }

            List<TypeData> typeDataList = new List<TypeData>(types.Count);
            foreach (Type type in types)
            {
                TypeData tyoeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
                typeDataList.Add(tyoeData);
            }

            return new TypeList(typeDataList);
        }
    }
}
