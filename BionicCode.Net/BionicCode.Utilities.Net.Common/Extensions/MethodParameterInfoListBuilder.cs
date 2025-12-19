namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class MethodParameterInfoListBuilder
    {
        internal static MethodParameterInfoList Create(IEnumerable<ParameterData> items)
        {
            List<ParameterData>? parameterDataList = items?.ToList();
            if (parameterDataList is null || parameterDataList.IsEmpty())
            {
                return MethodParameterInfoList.Empty;
            }

            List<MethodParameterInfo> parameters = new List<MethodParameterInfo>(parameterDataList.Count);
            MemberInfoData? member = null;
            foreach (ParameterData parameterData in parameterDataList)
            {
                if (member == null)
                {
                    member = parameterData.MemberData;
                }

                if (!ReferenceEquals(parameterData.MemberData, member))
                {
                    throw new ArgumentException("All MethodParameterInfo items must belong to the same member.");
                }

                var methodParameterInfo = new MethodParameterInfo(
                    parameterData.ParameterTypeHandle,
                    parameterData.Position,
                    parameterData.IsGenericMethodParameter,
                    parameterData.ParameterKind,
                    parameterData.DeclaringTypeHandle);
                parameters.Add(methodParameterInfo);
            }

            return new MethodParameterInfoList(parameters);
        }

        internal static MethodParameterInfoList Create(IEnumerable<ParameterInfo> items)
        {
            List<ParameterInfo>? parameterInfoList = items?.ToList();
            if (parameterInfoList is null || parameterInfoList.IsEmpty())
            {
                return MethodParameterInfoList.Empty;
            }

            List<MethodParameterInfo> parameters = new List<MethodParameterInfo>(parameterInfoList.Count);
            MemberInfoData? member = null;
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

                var methodParameterInfo = new MethodParameterInfo(
                    parameterData.ParameterTypeHandle,
                    parameterData.Position,
                    parameterData.IsGenericMethodParameter,
                    parameterData.ParameterKind,
                    parameterData.DeclaringTypeHandle);
                parameters.Add(methodParameterInfo);
            }

            return new MethodParameterInfoList(parameters);
        }
    }
}
