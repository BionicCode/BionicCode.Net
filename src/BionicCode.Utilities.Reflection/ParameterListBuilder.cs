namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal interface IParameterListBuilder
{
    ParameterizedMemberData DeclaringMember { get; }
    IParameterListBuilder Add(ParameterData parameterData);
    ParameterList Build();
}

internal class ParameterListBuilder : SymbolDataListBuilder<ParameterData>, IParameterListBuilder
{
    private ParameterList? _builderResult;
    private readonly ParameterizedMemberData _declaringMember;

    private ParameterListBuilder(ParameterizedMemberData declaringMember) : base(declaringMember.Handle) => _declaringMember = declaringMember;

    public static IParameterListBuilder New(ParameterizedMemberData declaringMember)
    {
        ArgumentNullException.ThrowIfNull(declaringMember);

        var builder = new ParameterListBuilder(declaringMember);
        return builder;
    }

    internal static ParameterList Create(IEnumerable<ParameterInfo>? items)
    {
        var parameterInfoList = items?.ToList();
        if (parameterInfoList is null || parameterInfoList.IsEmpty())
        {
            return ParameterList.Empty;
        }

        var parameters = new List<ParameterData>(parameterInfoList.Count);
        ParameterizedMemberData? member = null;
        foreach (ParameterInfo parameterInfo in parameterInfoList)
        {
            ParameterData parameterData = GetOrCreateCacheEntry(parameterInfo);

            member ??= parameterData.MemberData;

            if (!ReferenceEquals(parameterData.MemberData, member))
            {
                throw new ArgumentException($"All '{nameof(ParameterInfo)}' items must belong to the same member.");
            }

            parameters.Add(parameterData);
        }

        if (member is null)
        {
            throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: Unable to determine the declaring member of the provided '{nameof(ParameterInfo)}' items.");
        }

        return parameters.ToParameterList(member);
    }

    private static ParameterList CreateInternal(PropertyData propertyData, PropertyParameterSource propertyParameterSource)
    {
        ParameterList parameters = ParameterList.Empty;
        switch (propertyParameterSource)
        {
            case PropertyParameterSource.PropertySetMethod:
                {
                    if (!propertyData.CanWrite)
                    {
                        return parameters;
                    }

                    parameters = Create(propertyData.PropertySetMethodData);

                    break;
                }
            case PropertyParameterSource.PropertyGetMethod:
                {
                    if (!propertyData.CanRead)
                    {
                        return parameters;
                    }

                    parameters = Create(propertyData.PropertyGetMethodData);

                    break;
                }
        }

        return parameters;
    }

    internal static ParameterList Create(ParameterizedMemberData parameterizedMember)
    {
        ArgumentNullException.ThrowIfNull(parameterizedMember);

        ParameterInfo[] parameterInfoList = parameterizedMember.MethodBase.GetParameters();
        if (parameterInfoList.IsEmpty())
        {
            return ParameterList.Empty;
        }

        IEnumerable<ParameterData> parameters = parameterInfoList.Select(GetOrCreateCacheEntry);

        return parameters.ToParameterList(parameterizedMember);
    }

    ParameterizedMemberData IParameterListBuilder.DeclaringMember => _declaringMember;

    IParameterListBuilder IParameterListBuilder.Add(ParameterData parameterData)
    {
        Add(parameterData);
        return this;
    }

    ParameterList IParameterListBuilder.Build()
        => _builderResult ??= new ParameterList(Build(), ((IParameterListBuilder)this).DeclaringMember, isIntegrityValidationEnabled: false);

    private enum PropertyParameterSource
    {
        Undefined,
        PropertySetMethod,
        PropertyGetMethod,
    }
}

internal static class ParameterListBuilderExtensions
{
    internal static ParameterList ToParameterList(this IEnumerable<ParameterData>? items, ParameterizedMemberData parameterizedMember)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameterizedMember);

        return items is null || items.IsEmpty()
        ? ParameterList.Empty
        : new ParameterList(items, parameterizedMember);
    }

    internal static IParameterListView ToParameterListView(this IEnumerable<ParameterData> items, ParameterizedMemberData declaringMember)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringMember);

        return items is null || items.IsEmpty()
            ? ParameterListView.Empty
            : new ParameterListView(items.Select(item => item.View), declaringMember.View);
    }

    public static IParameterListView ToParameterListView(this IEnumerable<IParameterDataView> items, IParameterizedMemberDataView declaringMember)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringMember);

        return items is null || items.IsEmpty()
            ? ParameterListView.Empty
            : new ParameterListView(items.Select(item => item), declaringMember);
    }
}
