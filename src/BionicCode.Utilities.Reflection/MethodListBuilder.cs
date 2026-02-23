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

    public static IMethodListBuilder New(TypeData declaringType)
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

    internal static MethodList Create(TypeData declaringTypeData)
    {
        ArgumentNullException.ThrowIfNull(declaringTypeData);
        return CreateInternal(declaringTypeData);
    }

    internal static MethodList Create(Type declaringType)
    {
        ArgumentNullException.ThrowIfNull(declaringType);
        TypeData declaringTypeData = GetOrCreateCacheEntry(declaringType);
        return CreateInternal(declaringTypeData);
    }

    private static MethodList CreateInternal(TypeData declaringTypeData) => declaringTypeData.EnumerateMethods().ToMethodList(declaringTypeData);

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

    /// <summary>
    /// Returns an empty <see cref="MethodList"/> if the provided instance is <see langword="null"/>.
    /// </summary>
    /// <param name="items"></param>
    /// <returns>A <see cref="MethodList"/> that is empty if the provided instance is <see langword="null"/>. Otherwise, returns the original instance.</returns>
    public static MethodList OrEmpty(this MethodList items) => items ?? MethodList.Empty;
}
