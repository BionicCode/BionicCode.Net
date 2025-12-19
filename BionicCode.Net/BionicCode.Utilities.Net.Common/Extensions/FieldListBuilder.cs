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
            List<FieldInfo>? parameterInfoList = items?.ToList();
            if (parameterInfoList is null || parameterInfoList.IsEmpty())
            {
                return FieldList.Empty;
            }

            List<FieldData> parameters = new List<FieldData>(parameterInfoList.Count);
            RuntimeTypeHandle declaringTypeHandle = default;
            foreach (FieldInfo fieldInfo in parameterInfoList)
            {
                FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);

                if (declaringTypeHandle.Equals(default))
                {
                    declaringTypeHandle = fieldData.DeclaringTypeHandle;
                }

                if (!fieldData.DeclaringTypeHandle.Equals(declaringTypeHandle))
                {
                    throw new ArgumentException("All FieldInfo items must belong to the same member.");
                }

                parameters.Add(fieldData);
            }

            return new FieldList(parameters);
        }
    }
}
