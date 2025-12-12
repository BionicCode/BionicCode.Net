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
            MemberInfo member = null;
            foreach (ParameterInfo parameterInfo in items)
            {
                if (member == null)
                {
                    member = parameterInfo.Member;
                }

                if (parameterInfo.Member != member)
                {
                    throw new ArgumentException("All ParameterInfo items must belong to the same member.");
                }

                ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);
                parameters[index++] = parameterData;
            }

            return new ParameterList(parameters);
        }

        internal static class MethodParameterInfoListBuilder
        {
            internal static MethodParameterInfoList Create(IEnumerable<ParameterInfo> items)
            {
                ParameterData[] parameters = new ParameterData[items.Count()];
                int index = 0;
                MemberInfo member = null;
                foreach (ParameterInfo parameterInfo in items)
                {
                    if (member == null)
                    {
                        member = parameterInfo.Member;
                    }

                    if (parameterInfo.Member != member)
                    {
                        throw new ArgumentException("All ParameterInfo items must belong to the same member.");
                    }

                    ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);
                    parameters[index++] = parameterData;
                }

                return new ParameterList(parameters);
            }
        }
    }
