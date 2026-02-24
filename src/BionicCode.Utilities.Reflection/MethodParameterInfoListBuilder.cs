namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal interface IMethodParameterInfoListBuilder
{
    IMethodParameterInfoListBuilder Add(ParameterDescriptor methodParameterInfo);
    ParameterDescriptorList Build();
}

internal class MethodParameterInfoListBuilder : SymbolDataListBuilder<ParameterDescriptor>, IMethodParameterInfoListBuilder
{
    private ParameterDescriptorList? _builderResult;

    private MethodParameterInfoListBuilder(RuntimeTypeHandle declaringTypeHandle) : base(declaringTypeHandle)
    {
    }

    public static IMethodParameterInfoListBuilder New(RuntimeTypeHandle declaringTypeHandle)
    {
        var builder = new MethodParameterInfoListBuilder(declaringTypeHandle);
        return builder;
    }
    internal static ParameterDescriptorList Create(IEnumerable<ParameterData> items)
    {
        var parameterDataList = items?.ToList();
        if (parameterDataList is null || parameterDataList.IsEmpty())
        {
            return ParameterDescriptorList.Empty;
        }

        var parameters = new List<ParameterData>(parameterDataList.Count);
        SymbolInfoData? member = null;
        foreach (ParameterData parameterData in parameterDataList)
        {
            member ??= parameterData.MemberData;

            if (!ReferenceEquals(parameterData.MemberData, member))
            {
                throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(ParameterDescriptor)}' items must belong to the same member.");
            }

            parameters.Add(parameterData);
        }

        return parameters.AsMethodParameterInfoList();
    }
    internal static ParameterDescriptorList Create(ReadOnlySpan<ParameterData> items)
    {
        if (items.IsEmpty)
        {
            return ParameterDescriptorList.Empty;
        }

        var parameters = new List<ParameterData>(items.Length);
        SymbolInfoData? member = null;
        foreach (ParameterData parameterData in items)
        {
            member ??= parameterData.MemberData;

            if (!ReferenceEquals(parameterData.MemberData, member))
            {
                throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(ParameterDescriptor)}' items must belong to the same member.");
            }

            parameters.Add(parameterData);
        }

        return parameters.AsMethodParameterInfoList();
    }

    internal static ParameterDescriptorList Create(IEnumerable<ParameterDescriptor> items)
    {
        var parameterInfoList = items?.ToList();
        return parameterInfoList is null || parameterInfoList.IsEmpty()
            ? ParameterDescriptorList.Empty
            : new MethodParameterInfoList(parameterInfoList);
    }

    internal static ParameterDescriptorList Create(PropertyData propertyData)
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        return CreateInternal(propertyData.PropertyInfo);
    }

    internal static ParameterDescriptorList Create(PropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);

        return CreateInternal(propertyInfo);
    }

    internal static ParameterDescriptorList Create(MethodData methodData)
    {
        ArgumentNullException.ThrowIfNull(methodData);

        return CreateInternal(methodData.MethodInfo);
    }

    internal static ParameterDescriptorList Create(ConstructorData constructorData)
    {
        ArgumentNullException.ThrowIfNull(constructorData);

        return CreateInternal(constructorData.ConstructorInfo);
    }

    internal static ParameterDescriptorList Create(MethodBase methodBase)
    {
        ArgumentNullException.ThrowIfNull(methodBase);

        return CreateInternal(methodBase);
    }

    private static ParameterDescriptorList CreateInternal(MethodBase methodBase)
    {
        ParameterInfo[] parameterInfoList = methodBase.GetParameters();
        if (parameterInfoList.IsEmpty())
        {
            return ParameterDescriptorList.Empty;
        }

        IEnumerable<ParameterData> parameters = (IEnumerable<ParameterData>)parameterInfoList.Select(SymbolReflectionInfoCache.GetOrCreateCacheEntry);

        return parameters.AsMethodParameterInfoList();
    }

    private static ParameterDescriptorList CreateInternal(PropertyInfo propertyInfo)
    {
        ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
        if (indexParameters.IsEmpty())
        {
            return ParameterDescriptorList.Empty;
        }

        IEnumerable<ParameterData> parameters = (IEnumerable<ParameterData>)indexParameters.Select(SymbolReflectionInfoCache.GetOrCreateCacheEntry);
        return parameters.AsMethodParameterInfoList();
    }

    IMethodParameterInfoListBuilder IMethodParameterInfoListBuilder.Add(ParameterDescriptor methodParameterInfo)
    {
        Add(methodParameterInfo);
        return this;
    }

    ParameterDescriptorList IMethodParameterInfoListBuilder.Build()
        => _builderResult ??= new MethodParameterInfoList(Build(), isIntegrityValidationEnabled: false);
}

internal static class MethodParameterInfoListBuilderExtensions
{
    public static ParameterDescriptorList AsMethodParameterInfoList(this IEnumerable<ParameterData> items)
        => items is null || items.IsEmpty() ? ParameterDescriptorList.Empty : new MethodParameterInfoList(items.Select(parameterData => new ParameterDescriptor(parameterData)));

    /// <summary>
    /// Returns an empty <see cref="MethodParameterInfoList"/> if the provided instance is <see langword="null"/>.
    /// </summary>
    /// <param name="items"></param>
    /// <returns>A <see cref="MethodParameterInfoList"/> that is empty if the provided instance is <see langword="null"/>. Otherwise, returns the original instance.</returns>
    //public static ParameterDescriptorList OrEmpty(this ParameterDescriptorList items)
    //    => items ?? ParameterDescriptorList.Empty;
}
