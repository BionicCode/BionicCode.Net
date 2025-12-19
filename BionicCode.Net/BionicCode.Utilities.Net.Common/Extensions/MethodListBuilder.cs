namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class MethodListBuilder
    {
        internal static MethodList Create(IEnumerable<MethodInfo>? items)
        {
            List<MethodInfo>? parameterInfoList = items?.ToList();
            if (parameterInfoList is null || parameterInfoList.IsEmpty())
            {
                return MethodList.Empty;
            }

            List<MethodData> parameters = new List<MethodData>(parameterInfoList.Count);
            RuntimeTypeHandle declaringTypeHandle = default;
            foreach (MethodInfo parameterInfo in parameterInfoList)
            {
                MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);

                if (declaringTypeHandle.Equals(default))
                {
                    declaringTypeHandle = methodData.DeclaringTypeHandle;
                }

                if (!methodData.DeclaringTypeHandle.Equals(declaringTypeHandle))
                {
                    throw new ArgumentException("All MethodInfo items must belong to the same member.");
                }

                parameters.Add(methodData);
            }

            return new MethodList(parameters);
        }
    }
}
