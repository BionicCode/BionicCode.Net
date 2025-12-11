namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class ParameterListBuilder
    {
        internal static ParameterList Create(IEnumerable<ParameterInfo> items)
        {
            ParameterData[] parameters = new ParameterData[items.Count()];
            int index = 0;
            foreach (ParameterInfo parameterInfo in items)
            {
                ParameterData parameterDatda = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);
                parameters[index++] = parameterDatda;
            }

            return new ParameterList(parameters);
        }
    }
}
