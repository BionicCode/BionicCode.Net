namespace BionicCode.Utilities.Net.Reflection;

using BionicCode.Utilities.Net;

public interface IPropertyDataView : IMemberDataView, ISymbolInfoDataView
{
    bool CanRead { get; }
    bool CanWrite { get; }
    AccessModifier GetAccessorAccessModifier { get; }
    bool IsIndexer { get; }
    bool IsInit { get; }
    bool IsOverride { get; }
    bool IsReadOnly { get; }
    bool IsSealed { get; }
    IMethodDataView PropertyGetMethod { get; }
    IParameterListView PropertyGetMethodParameters { get; }
    IMethodDataView PropertySetMethod { get; }
    IParameterListView PropertySetMethodParameters { get; }
    ITypeDataView PropertyType { get; }
    AccessModifier SetAccessorAccessModifier { get; }

    object? GetIndexerValue(object? target, params object?[] indexerPropertyParameters);
    bool TryGetIndexerValue(object? target, out object? propertyValue, params object?[] indexerPropertyParameters);
    TValue GetIndexerValue<TTarget, TValue, TIndex>(TTarget target, params TIndex[] indexerPropertyParameters);
    bool TryGetIndexerValue<TTarget, TValue, TIndex>(TTarget target, out TValue propertyValue, params TIndex[] indexerPropertyParameters);
    TValue GetIndexerValue<TTarget, TValue>(TTarget target, params object?[] indexerPropertyParameters);
    bool TryGetIndexerValue<TTarget, TValue>(TTarget target, out TValue propertyValue, params object?[] indexerPropertyParameters);
    TValue GetIndexerValue<TTarget, TValue, TIndex1, TIndex2>(TTarget target, TIndex1 index1, TIndex2 index2);
    bool TryGetIndexerValue<TTarget, TValue, TIndex1, TIndex2>(TTarget target, TIndex1 index1, TIndex2 index2, out TValue propertyValue);
    TValue GetIndexerValue<TTarget, TValue, TIndex1, TIndex2, TIndex3>(TTarget target, TIndex1 index1, TIndex2 index2, TIndex3 index3);
    bool TryGetIndexerValue<TTarget, TValue, TIndex1, TIndex2, TIndex3>(TTarget target, TIndex1 index1, TIndex2 index2, TIndex3 index3, out TValue propertyValue);
    object? GetValue(object? target);
    bool TryGetValue(object? target, out object? propertyValue);
    TValue GetValue<TTarget, TValue>(TTarget target);
    bool TryGetValue<TTarget, TValue>(TTarget target, out TValue propertyValue);
    void SetIndexerValue(object? target, object? value, params object?[] indexerPropertyIndex);
    bool TrySetIndexerValue(object? target, object? value, params object?[] indexerPropertyIndex);
    void SetStructValue<TTarget, TValue>(ref TTarget target, TValue value, params object?[] indexerPropertyIndex) where TTarget : struct;
    bool TrySetStructValue<TTarget, TValue>(ref TTarget target, TValue value, params object?[] indexerPropertyIndex) where TTarget : struct;
    void SetValue(object? target, object? value);
    bool TrySetValue(object? target, object? value);
    void SetValue<TTarget, TValue>(TTarget target, TValue value) where TTarget : class;
    bool TrySetValue<TTarget, TValue>(TTarget target, TValue value) where TTarget : class;
    void SetStructIndexerValue<TTarget, TValue>(ref TTarget target, TValue value, params object?[] indexerPropertyIndex) where TTarget : struct;
    bool TrySetStructIndexerValue<TTarget, TValue>(ref TTarget target, TValue value, params object?[] indexerPropertyIndex) where TTarget : struct;
    void Set3DIndexerValue<TTarget, TValue, TIndex1, TIndex2, TIndex3>(TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3);
    bool TrySet3DIndexerValue<TTarget, TValue, TIndex1, TIndex2, TIndex3>(TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3);
    void Set2DIndexerValue<TTarget, TValue, TIndex1, TIndex2>(TTarget target, TValue value, TIndex1 index1, TIndex2 index2);
    bool TrySet2DIndexerValue<TTarget, TValue, TIndex1, TIndex2>(TTarget target, TValue value, TIndex1 index1, TIndex2 index2);
    void SetIndexerValue<TTarget, TValue, TIndex1>(TTarget target, TValue value, TIndex1 index1);
    bool TrySetIndexerValue<TTarget, TValue, TIndex1>(TTarget target, TValue value, TIndex1 index1);
    void SetStruct3DIndexerValue<TTarget, TValue, TIndex1, TIndex2, TIndex3>(ref TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3) where TTarget : struct;
    bool TrySetStruct3DIndexerValue<TTarget, TValue, TIndex1, TIndex2, TIndex3>(ref TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3) where TTarget : struct;
    void SetStruct2DIndexerValue<TTarget, TValue, TIndex1, TIndex2>(ref TTarget target, TValue value, TIndex1 index1, TIndex2 index2) where TTarget : struct;
    bool TrySetStruct2DIndexerValue<TTarget, TValue, TIndex1, TIndex2>(ref TTarget target, TValue value, TIndex1 index1, TIndex2 index2) where TTarget : struct;
    void SetStructIndexerValue<TTarget, TValue, TIndex1>(ref TTarget target, TValue value, TIndex1 index1) where TTarget : struct;
    bool TrySetStructIndexerValue<TTarget, TValue, TIndex1>(ref TTarget target, TValue value, TIndex1 index1) where TTarget : struct;
}
