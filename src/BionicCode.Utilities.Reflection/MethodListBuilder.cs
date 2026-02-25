namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal interface IMethodListBuilder
{
    TypeData DeclaringType { get; }
    IMethodListBuilder Add(MethodData propertyData);
    MethodList Build();
}

internal class MethodListBuilder : SymbolDataListBuilder<MethodData>, IMethodListBuilder
{
    private MethodList? _builderResult;
    private readonly TypeData _declaringType;

    private MethodListBuilder(TypeData declaringType) : base(declaringType.Handle) => _declaringType = declaringType;

    public static IMethodListBuilder New(TypeData declaringType, MemberEnumerationRule enumerationRule)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);
        var builder = new MethodListBuilder(declaringType);
        return builder;
    }
    internal static MethodList Create(IEnumerable<MethodInfo>? items)
    {
        var methodInfoList = items?.ToList();
        if (methodInfoList is null || methodInfoList.IsEmpty())
        {
            return MethodList.Empty;
        }

        var methods = new List<MethodData>(methodInfoList.Count);
        TypeData? declaringTypeData = default;
        foreach (MethodInfo methodInfo in methodInfoList)
        {
            MethodData methodData = GetOrCreateCacheEntry(methodInfo);

            declaringTypeData ??= methodData.DeclaringTypeData;

            if (!ReferenceEquals(methodData.DeclaringTypeData, declaringTypeData))
            {
                throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(MethodInfo)}' items must belong to the same declaring type.");
            }

            methods.Add(methodData);
        }

        if (declaringTypeData is null)
        {
            throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: Unable to determine the declaring type of the provided '{nameof(MethodInfo)}' items.");
        }

        return methods.ToMethodList(declaringTypeData);
    }

    internal static MethodList Create(TypeData declaringTypeData, MemberEnumerationRule enumerationRule)
    {
        ArgumentNullException.ThrowIfNull(declaringTypeData);
        return CreateInternal(declaringTypeData, enumerationRule);
    }

    internal static MethodList Create(Type declaringType, MemberEnumerationRule enumerationRule)
    {
        ArgumentNullException.ThrowIfNull(declaringType);
        TypeData declaringTypeData = GetOrCreateCacheEntry(declaringType);
        return CreateInternal(declaringTypeData, enumerationRule);
    }

    private static MethodList CreateInternal(TypeData declaringTypeData, MemberEnumerationRule enumerationRule) => declaringTypeData.EnumerateMethods(enumerationRule).ToMethodList(declaringTypeData);

    TypeData IMethodListBuilder.DeclaringType => _declaringType;

    public TypeData DeclaringType { get; }

    IMethodListBuilder IMethodListBuilder.Add(MethodData methodData)
    {
        Add(methodData);
        return this;
    }

    MethodList IMethodListBuilder.Build()
        => _builderResult ??= new MethodList(Build(), ((IMethodListBuilder)this).DeclaringType, isIntegrityValidationEnabled: false);
}

internal static class MethodListBuilderExtensions
{
    public static MethodList ToMethodList(this IEnumerable<MethodData> items, TypeData declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        return items is null || items.IsEmpty()
            ? MethodList.Empty
            : new MethodList(items, declaringType);
    }

    internal static IMethodListView ToMethodListView(this IEnumerable<MethodData> items, TypeData declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        return items is null || items.IsEmpty()
            ? MethodListView.Empty
            : new MethodListView(items.Select(item => item.View), declaringType.View);
    }

    public static IMethodListView ToMethodListView(this IEnumerable<IMethodDataView> items, ITypeDataView declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        return items is null || items.IsEmpty()
            ? MethodListView.Empty
            : new MethodListView(items.Select(item => item), declaringType);
    }
}
