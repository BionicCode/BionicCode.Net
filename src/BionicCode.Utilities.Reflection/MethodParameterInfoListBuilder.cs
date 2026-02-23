namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal interface IMethodParameterInfoListBuilder
{
    IMethodParameterInfoListBuilder Add(MethodParameterInfo methodParameterInfo);
    MethodParameterInfoList Build();
}

internal class MethodParameterInfoListBuilder : SymbolDataListBuilder<MethodParameterInfo>, IMethodParameterInfoListBuilder
{
    private MethodParameterInfoList? _builderResult;

    private MethodParameterInfoListBuilder(RuntimeTypeHandle declaringTypeHandle) : base(declaringTypeHandle)
    {
    }

    public static IMethodParameterInfoListBuilder New(RuntimeTypeHandle declaringTypeHandle)
    {
        var builder = new MethodParameterInfoListBuilder(declaringTypeHandle);
        return builder;
    }
    internal static MethodParameterInfoList Create(IEnumerable<ParameterData> items)
    {
        var parameterDataList = items?.ToList();
        if (parameterDataList is null || parameterDataList.IsEmpty())
        {
            return MethodParameterInfoList.Empty;
        }

        var parameters = new List<ParameterData>(parameterDataList.Count);
        SymbolInfoData? member = null;
        foreach (ParameterData parameterData in parameterDataList)
        {
            member ??= parameterData.MemberData;

            if (!ReferenceEquals(parameterData.MemberData, member))
            {
                throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(MethodParameterInfo)}' items must belong to the same member.");
            }

            parameters.Add(parameterData);
        }

        return parameters.AsMethodParameterInfoList();
    }
    internal static MethodParameterInfoList Create(ReadOnlySpan<ParameterData> items)
    {
        if (items.IsEmpty)
        {
            return MethodParameterInfoList.Empty;
        }

        var parameters = new List<ParameterData>(items.Length);
        SymbolInfoData? member = null;
        foreach (ParameterData parameterData in items)
        {
            member ??= parameterData.MemberData;

            if (!ReferenceEquals(parameterData.MemberData, member))
            {
                throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(MethodParameterInfo)}' items must belong to the same member.");
            }

            parameters.Add(parameterData);
        }

        return parameters.AsMethodParameterInfoList();
    }

    internal static MethodParameterInfoList Create(IEnumerable<MethodParameterInfo> items)
    {
        var parameterInfoList = items?.ToList();
        return parameterInfoList is null || parameterInfoList.IsEmpty()
            ? MethodParameterInfoList.Empty
            : new MethodParameterInfoList(parameterInfoList);
    }

    internal static MethodParameterInfoList Create(PropertyData propertyData)
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        return CreateInternal(propertyData.PropertyInfo);
    }

    internal static MethodParameterInfoList Create(PropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);

        return CreateInternal(propertyInfo);
    }

    internal static MethodParameterInfoList Create(MethodData methodData)
    {
        ArgumentNullException.ThrowIfNull(methodData);

        return CreateInternal(methodData.MethodInfo);
    }

    internal static MethodParameterInfoList Create(ConstructorData constructorData)
    {
        ArgumentNullException.ThrowIfNull(constructorData);

        return CreateInternal(constructorData.ConstructorInfo);
    }

    internal static MethodParameterInfoList Create(MethodBase methodBase)
    {
        ArgumentNullException.ThrowIfNull(methodBase);

        return CreateInternal(methodBase);
    }

    private static MethodParameterInfoList CreateInternal(MethodBase methodBase)
    {
        ParameterInfo[] parameterInfoList = methodBase.GetParameters();
        if (parameterInfoList.IsEmpty())
        {
            return MethodParameterInfoList.Empty;
        }

        IEnumerable<ParameterData> parameters = (IEnumerable<ParameterData>)parameterInfoList.Select(SymbolReflectionInfoCache.GetOrCreateCacheEntry);

        return parameters.AsMethodParameterInfoList();
    }

    private static MethodParameterInfoList CreateInternal(PropertyInfo propertyInfo)
    {
        ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
        if (indexParameters.IsEmpty())
        {
            return MethodParameterInfoList.Empty;
        }

        IEnumerable<ParameterData> parameters = (IEnumerable<ParameterData>)indexParameters.Select(SymbolReflectionInfoCache.GetOrCreateCacheEntry);
        return parameters.AsMethodParameterInfoList();
    }

    IMethodParameterInfoListBuilder IMethodParameterInfoListBuilder.Add(MethodParameterInfo methodParameterInfo)
    {
        Add(methodParameterInfo);
        return this;
    }

    MethodParameterInfoList IMethodParameterInfoListBuilder.Build()
        => _builderResult ??= new MethodParameterInfoList(Build(), isIntegrityValidationEnabled: false);
}

internal static class MethodParameterInfoListBuilderExtensions
{
    public static MethodParameterInfoList AsMethodParameterInfoList(this IEnumerable<ParameterData> items)
        => items is null || items.IsEmpty() ? MethodParameterInfoList.Empty : new MethodParameterInfoList(items.Select(parameterData => new MethodParameterInfo(parameterData)));

    /// <summary>
    /// Returns an empty <see cref="MethodParameterInfoList"/> if the provided instance is <see langword="null"/>.
    /// </summary>
    /// <param name="items"></param>
    /// <returns>A <see cref="MethodParameterInfoList"/> that is empty if the provided instance is <see langword="null"/>. Otherwise, returns the original instance.</returns>
    //public static MethodParameterInfoList OrEmpty(this MethodParameterInfoList items)
    //    => items ?? MethodParameterInfoList.Empty;
}
