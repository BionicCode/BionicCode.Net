namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal interface IFieldListBuilder
{
    TypeData DeclaringType { get; }
    IFieldListBuilder Add(FieldData fieldData);
    FieldList Build();
}

internal class FieldListBuilder : SymbolDataListBuilder<FieldData>, IFieldListBuilder
{
    private FieldList? _builderResult;
    private readonly TypeData _declaringType;

    public TypeData DeclaringType { get; }

    private FieldListBuilder(TypeData declaringType) : base(declaringType.Handle) => _declaringType = declaringType;

    public static IFieldListBuilder New(TypeData declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        var builder = new FieldListBuilder(declaringType);
        return builder;
    }

    internal static FieldList Create(IEnumerable<FieldInfo>? items)
    {
        var fieldInfoList = items?.ToList();
        if (fieldInfoList is null || fieldInfoList.IsEmpty())
        {
            return FieldList.Empty;
        }

        var fields = new List<FieldData>(fieldInfoList.Count);
        TypeData? declaringTypeData = default;
        foreach (FieldInfo fieldInfo in fieldInfoList)
        {
            FieldData fieldData = GetOrCreateCacheEntry(fieldInfo);

            declaringTypeData ??= fieldData.DeclaringTypeData;

            if (!ReferenceEquals(fieldData.DeclaringTypeData, declaringTypeData))
            {
                throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(FieldInfo)}' items must belong to the same declaring type.");
            }

            fields.Add(fieldData);
        }

        if (declaringTypeData is null)
        {
            throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: Unable to determine the declaring type of the provided '{nameof(FieldInfo)}' items.");
        }

        return fields.ToFieldList(declaringTypeData);
    }

    internal static FieldList Create(TypeData declaringTypeData)
    {
        ArgumentNullException.ThrowIfNull(declaringTypeData);
        return CreateInternal(declaringTypeData);
    }

    internal static FieldList Create(Type declaringType)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        TypeData declaringTypeData = GetOrCreateCacheEntry(declaringType);
        return CreateInternal(declaringTypeData);
    }

    private static FieldList CreateInternal(TypeData declaringTypeData) => declaringTypeData.EnumerateFields().ToFieldList(declaringTypeData);

    IFieldListBuilder IFieldListBuilder.Add(FieldData fieldData)
    {
        Add(fieldData);
        return this;
    }

    FieldList IFieldListBuilder.Build()
        => _builderResult ??= new FieldList(Build(), ((IFieldListBuilder)this).DeclaringType, isIntegrityValidationEnabled: false);
}

internal static class FieldListBuilderExtensions
{
    internal static FieldList ToFieldList(this IEnumerable<FieldData> items, TypeData declaringTypeData)
        => items is null || items.IsEmpty() ? FieldList.Empty : new FieldList(items, declaringTypeData);

    /// <summary>
    /// Returns an empty <see cref="FieldList"/> if the provided instance is <see langword="null"/>.
    /// </summary>
    /// <param name="items"></param>
    /// <returns>A <see cref="FieldList"/> that is empty if the provided instance is <see langword="null"/>. Otherwise, returns the original instance.</returns>
    public static FieldList OrEmpty(this FieldList items)
        => items ?? FieldList.Empty;
}
