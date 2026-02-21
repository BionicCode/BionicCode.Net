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

    public static PropertyList Create(TypeData declaringTypeData)
    {
        ArgumentNullException.ThrowIfNull(declaringTypeData);
        return CreateInternal(declaringTypeData);
    }

    public static PropertyList Create(Type declaringType)
    {
        ArgumentNullException.ThrowIfNull(declaringType);
        TypeData declaringTypeData = GetOrCreateCacheEntry(declaringType);
        return CreateInternal(declaringTypeData);
    }

    private static PropertyList CreateInternal(TypeData declaringTypeData) => declaringTypeData.EnumerateProperties().ToPropertyList(declaringTypeData);

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
    public static PropertyList ToPropertyList(this IEnumerable<PropertyData> items, TypeData declaringTypeData)
        => items is null || items.IsEmpty() ? PropertyList.Empty : new PropertyList(items, declaringTypeData);

    /// <summary>
    /// Returns an empty <see cref="PropertyList"/> if the provided instance is <see langword="null"/>.
    /// </summary>
    /// <param name="items"></param>
    /// <returns>A <see cref="PropertyList"/> that is empty if the provided instance is <see langword="null"/>. Otherwise, returns the original instance.</returns>
    public static PropertyList OrEmpty(this PropertyList items)
        => items ?? PropertyList.Empty;
}
