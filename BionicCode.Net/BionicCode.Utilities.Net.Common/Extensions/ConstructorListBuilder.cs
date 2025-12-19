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
            List<ConstructorInfo>? parameterInfoList = items?.ToList();
            if (parameterInfoList is null || parameterInfoList.IsEmpty())
            {
                return ConstructorList.Empty;
            }

            List<ConstructorData> parameters = new List<ConstructorData>(parameterInfoList.Count);
            RuntimeTypeHandle declaringTypeHandle = default;
            foreach (ConstructorInfo constructocInfo in parameterInfoList)
            {
                ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructocInfo);

                if (declaringTypeHandle.Equals(default))
                {
                    declaringTypeHandle = constructorData.DeclaringTypeHandle;
                }

                if (!constructorData.DeclaringTypeHandle.Equals(declaringTypeHandle))
                {
                    throw new ArgumentException("All ConstructorInfo items must belong to the same member.");
                }

                parameters.Add(constructorData);
            }

            return new ConstructorList(parameters);
        }
    }
}
