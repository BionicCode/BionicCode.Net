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

    internal static FieldList Create(TypeData declaringTypeData, MemberEnumerationRule enumerationRule)
    {
        ArgumentNullException.ThrowIfNull(declaringTypeData);
        return CreateInternal(declaringTypeData, enumerationRule);
    }

    internal static FieldList Create(Type declaringType, MemberEnumerationRule enumerationRule)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        TypeData declaringTypeData = GetOrCreateCacheEntry(declaringType);
        return CreateInternal(declaringTypeData, enumerationRule);
    }

    private static FieldList CreateInternal(TypeData declaringTypeData, MemberEnumerationRule enumerationRule) => declaringTypeData.EnumerateFields(enumerationRule).ToFieldList(declaringTypeData);

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
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringTypeData);

        return items is null || items.IsEmpty()
            ? FieldList.Empty
            : new FieldList(items, declaringTypeData);
    }

    internal static IFieldListView ToFieldListView(this IEnumerable<FieldData> items, TypeData declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        return items is null || items.IsEmpty()
            ? FieldListView.Empty
            : new FieldListView(items.Select(item => item.View), declaringType.View);
    }

    public static IFieldListView ToFieldListView(this IEnumerable<IFieldDataView> items, ITypeDataView declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        return items is null || items.IsEmpty()
            ? FieldListView.Empty
            : new FieldListView(items.Select(item => item), declaringType);
    }
}
