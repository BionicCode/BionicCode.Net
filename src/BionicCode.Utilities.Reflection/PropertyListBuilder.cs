namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal interface IPropertyListBuilder
{
    TypeData DeclaringType { get; }
    IPropertyListBuilder Add(PropertyData propertyData);
    PropertyList Build();
}

internal class PropertyListBuilder : SymbolDataListBuilder<PropertyData>, IPropertyListBuilder
{
    private PropertyList? _builderResult;
    private readonly TypeData _declaringType;

    private PropertyListBuilder(TypeData declaringType)
        : base(declaringType.Handle) => _declaringType = declaringType;

    public static IPropertyListBuilder New(TypeData declaringType)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        var builder = new PropertyListBuilder(declaringType);
        return builder;
    }

    public static PropertyList Create(IEnumerable<PropertyInfo>? items)
    {
        var propertyInfoList = items?.ToList();
        if (propertyInfoList is null || propertyInfoList.IsEmpty())
        {
            return PropertyList.Empty;
        }

        var properties = new List<PropertyData>(propertyInfoList.Count);
        TypeData? declaringTypeData = default;
        foreach (PropertyInfo propertyInfo in propertyInfoList)
        {
            PropertyData propertyData = GetOrCreateCacheEntry(propertyInfo);

            declaringTypeData ??= propertyData.DeclaringTypeData;

            if (!ReferenceEquals(propertyData.DeclaringTypeData, declaringTypeData))
            {
                throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(PropertyInfo)}' items must belong to the same declaring type.");
            }

            properties.Add(propertyData);
        }

        if (declaringTypeData is null)
        {
            throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: Unable to determine the declaring type of the provided '{nameof(PropertyInfo)}' items.");
        }

        return properties.ToPropertyList(declaringTypeData);
    }

    public static PropertyList Create(TypeData declaringTypeData, MemberEnumerationRule enumerationRule)
    {
        ArgumentNullException.ThrowIfNull(declaringTypeData);
        return CreateInternal(declaringTypeData, enumerationRule);
    }

    public static PropertyList Create(Type declaringType, MemberEnumerationRule enumerationRule)
    {
        ArgumentNullException.ThrowIfNull(declaringType);
        TypeData declaringTypeData = GetOrCreateCacheEntry(declaringType);
        return CreateInternal(declaringTypeData, enumerationRule);
    }

    private static PropertyList CreateInternal(TypeData declaringTypeData, MemberEnumerationRule enumerationRule) => declaringTypeData.EnumerateProperties(enumerationRule).ToPropertyList(declaringTypeData);

    TypeData IPropertyListBuilder.DeclaringType => _declaringType;

    IPropertyListBuilder IPropertyListBuilder.Add(PropertyData propertyData)
    {
        Add(propertyData);
        return this;
    }

    PropertyList IPropertyListBuilder.Build()
        => _builderResult ??= new PropertyList(Build(), ((IPropertyListBuilder)this).DeclaringType, isIntegrityValidationEnabled: false);
}

internal static class PropertyListBuilderExtensions
{
    public static PropertyList ToPropertyList(this IEnumerable<PropertyData> items, TypeData declaringTypeData) => items is null || items.IsEmpty()
        ? PropertyList.Empty
        : new PropertyList(items, declaringTypeData);

    internal static IPropertyListView ToPropertyListView(this IEnumerable<PropertyData> items, TypeData declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        return items is null || items.IsEmpty()
            ? PropertyListView.Empty
            : new PropertyListView(items.Select(item => item.View), declaringType.View);
    }

    public static IPropertyListView ToPropertyListView(this IEnumerable<IPropertyDataView> items, ITypeDataView declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        return items is null || items.IsEmpty()
            ? PropertyListView.Empty
            : new PropertyListView(items.Select(item => item), declaringType);
    }
}
