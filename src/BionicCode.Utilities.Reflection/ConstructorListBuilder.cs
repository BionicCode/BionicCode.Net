namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal interface IConstructorListBuilder
{
    TypeData DeclaringType { get; }
    IConstructorListBuilder Add(ConstructorData constructorData);
    ConstructorList Build();
}

internal class ConstructorListBuilder : SymbolDataListBuilder<ConstructorData>, IConstructorListBuilder
{
    private ConstructorList? _builderResult;
    private readonly TypeData _declaringType;

    private ConstructorListBuilder(TypeData declaringType) : base(declaringType.Handle) => _declaringType = declaringType;

    public static IConstructorListBuilder New(TypeData declaringType)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        var builder = new ConstructorListBuilder(declaringType);
        return builder;
    }

    internal static ConstructorList Create(IEnumerable<ConstructorInfo>? items)
    {
        var constructorInfoList = items?.ToList();
        if (constructorInfoList is null || constructorInfoList.IsEmpty())
        {
            return ConstructorList.Empty;
        }

        var constructors = new List<ConstructorData>(constructorInfoList.Count);
        TypeData? declaringType = null;
        foreach (ConstructorInfo constructorInfo in constructorInfoList)
        {
            ConstructorData constructorData = GetOrCreateCacheEntry(constructorInfo);

            declaringType ??= constructorData.DeclaringTypeData;

            if (!ReferenceEquals(constructorData.DeclaringTypeData, declaringType))
            {
                throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(ConstructorInfo)}' items must belong to the same declaring type.");
            }

            constructors.Add(constructorData);
        }

        return constructors.ToConstructorList(declaringType);
    }

    internal static ConstructorList Create(TypeData declaringTypeData)
    {
        ArgumentNullException.ThrowIfNull(declaringTypeData);

        return CreateInternal(declaringTypeData);
    }

    internal static ConstructorList Create(Type declaringType)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        TypeData declaringTypeData = GetOrCreateCacheEntry(declaringType);
        return CreateInternal(declaringTypeData);
    }

    private static ConstructorList CreateInternal(TypeData declaringType) => declaringType.EnumerateConstructors().ToConstructorList(declaringType);

    TypeData IConstructorListBuilder.DeclaringType => _declaringType;

    IConstructorListBuilder IConstructorListBuilder.Add(ConstructorData constructorData)
    {
        Add(constructorData);
        return this;
    }

    ConstructorList IConstructorListBuilder.Build()
        => _builderResult ??= new ConstructorList(Build(), ((IConstructorListBuilder)this).DeclaringType, isIntegrityValidationEnabled: false);
}

internal static class ConstructorListBuilderExtensions
{
    internal static ConstructorList ToConstructorList(this IEnumerable<ConstructorData> items, TypeData? declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        return items is null || items.IsEmpty()
            ? ConstructorList.Empty
            : new ConstructorList(items, declaringType);
    }

    internal static IConstructorListView ToConstructorListView(this IEnumerable<ConstructorData> items, TypeData declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        return items is null || items.IsEmpty()
            ? ConstructorListView.Empty
            : new ConstructorListView(items.Select(item => item.View), declaringType.View);
    }

    public static IConstructorListView ToConstructorListView(this IEnumerable<IConstructorDataView> items, ITypeDataView declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        return items is null || items.IsEmpty()
            ? ConstructorListView.Empty
            : new ConstructorListView(items.Select(item => item), declaringType);
    }

    /// <summary>
    /// Returns an empty <see cref="ConstructorList"/> if the provided instance is <see langword="null"/>.
    /// </summary>
    /// <param name="items"></param>
    /// <returns>A <see cref="ConstructorList"/> that is empty if the provided instance is <see langword="null"/>. Otherwise, returns the original instance.</returns>
    public static ConstructorList OrEmpty(this ConstructorList items)
        => items ?? ConstructorList.Empty;
}
