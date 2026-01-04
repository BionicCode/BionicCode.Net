namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class ParameterListBuilder
    {
        internal static ParameterList Create(IEnumerable<ParameterInfo>? items)
        {
            List<ParameterInfo>? parameterInfoList = items?.ToList();
            if (parameterInfoList is null || parameterInfoList.IsEmpty())
            {
                return ParameterList.Empty;
            }

            List<ParameterData> parameters = new List<ParameterData>(parameterInfoList.Count);
            MemberData? member = null;
            foreach (ParameterInfo parameterInfo in parameterInfoList)
            {
                ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);

                if (member == null)
                {
                    member = parameterData.MemberData;
                }

                if (!ReferenceEquals(parameterData.MemberData, member))
                {
                    throw new ArgumentException("All ParameterInfo items must belong to the same member.");
                }

                parameters.Add(parameterData);
            }

            return new ParameterList(parameters);
        }
    }
}
