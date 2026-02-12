namespace BionicCode.Utilities.Net.Reflection;

using System.Reflection;
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
    bool IsSetMethodReadOnly { get; }
    IMethodDataView PropertyGetMethodData { get; }
    IParameterListView PropertyGetMethodParameters { get; }
    IMethodDataView PropertySetMethodData { get; }
    IParameterListView PropertySetMethodParameters { get; }
    ITypeDataView PropertyTypeData { get; }
    AccessModifier SetAccessorAccessModifier { get; }

    object? GetIndexerValue(object? target, object?[] indexerPropertyParameters);
    TValue GetIndexerValue<TTarget, TValue, TIndex>(TTarget target, params TIndex[] indexerPropertyParameters);
    TValue GetIndexerValue<TTarget, TValue>(TTarget target, params object[] indexerPropertyParameters);
    object? GetValue(object? target);
    TValue GetValue<TTarget, TValue>(TTarget target);
    void SetIndexerValue(object? target, object? value, object?[]? indexerPropertyIndex = null);
    void SetStructValue<TTarget, TValue>(ref TTarget target, TValue value, object[]? indexerPropertyIndex = null) where TTarget : struct;
    void SetValue(object? target, object? value);
    void SetValue<TTarget, TValue>(TTarget target, TValue value) where TTarget : class;
}
