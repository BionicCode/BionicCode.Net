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

public class PropertyDataView : IPropertyDataView
{
    public bool CanRead { get; }
    public bool CanWrite { get; }
    public AccessModifier GetAccessorAccessModifier { get; }
    public bool IsIndexer { get; }
    public bool IsInit { get; }
    public bool IsOverride { get; }
    public bool IsReadOnly { get; }
    public bool IsSealed { get; }
    public bool IsSetMethodReadOnly { get; }
    public IMethodDataView? PropertyGetMethodData { get; }
    public IParameterListView PropertyGetMethodParameters { get; }
    public IMethodDataView? PropertySetMethodData { get; }
    public IParameterListView PropertySetMethodParameters { get; }
    public ITypeDataView? PropertyTypeData { get; }
    public AccessModifier SetAccessorAccessModifier { get; }
    public AccessModifier AccessModifier { get; }
    public BindingFlags BindingFlagsVisibilityMask { get; }
    public ITypeDataView? DeclaringTypData { get; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; }
    public RuntimeTypeHandle ImplementingTypeHandle { get; }
    public ITypeDataView? ImplementingTypData { get; }
    public bool IsAssembly { get; }
    public bool IsExplicitInterfaceImplementation { get; }
    public bool IsFamily { get; }
    public bool IsFamilyAndAssembly { get; }
    public bool IsFamilyOrAssembly { get; }
    public bool IsPrivate { get; }
    public bool IsPublic { get; }
    public bool IsStatic { get; }
    public string? AssemblyName { get; }
    public string? Namespace { get; }
    public IList<CustomAttributeData>? AttributeData { get; }
    public string? DisplayName { get; }
    public int FormattingIndentation { get; set; }
    public string? FullyQualifiedDisplayName { get; }
    public string? FullyQualifiedRuntimeSignature { get; }
    public string? FullyQualifiedSignature { get; }
    public string? IndentationString { get; }
    public string? Name { get; }
    public string? RuntimeShortCompactSignature { get; }
    public string? RuntimeShortSignature { get; }
    public string? RuntimeSignature { get; }
    public string? ShortCompactSignature { get; }
    public string? ShortDisplayName { get; }
    public string? ShortSignature { get; }
    public string? Signature { get; }
    public SymbolAttributes SymbolAttributes { get; }
    public SymbolComponentInfo? SymbolComponentInfo { get; }
    public SymbolKind SymbolKind { get; }

    public object? GetIndexerValue(object? target, object?[] indexerPropertyParameters) => throw new NotImplementedException();
    public TValue GetIndexerValue<TTarget, TValue, TIndex>(TTarget target, params TIndex[] indexerPropertyParameters) => throw new NotImplementedException();
    public TValue GetIndexerValue<TTarget, TValue>(TTarget target, params object[] indexerPropertyParameters) => throw new NotImplementedException();
    public object? GetValue(object? target) => throw new NotImplementedException();
    public TValue GetValue<TTarget, TValue>(TTarget target) => throw new NotImplementedException();
    public void SetIndexerValue(object? target, object? value, object?[]? indexerPropertyIndex = null) => throw new NotImplementedException();
    public void SetStructValue<TTarget, TValue>(ref TTarget target, TValue value, object[]? indexerPropertyIndex = null) where TTarget : struct => throw new NotImplementedException();
    public void SetValue(object? target, object? value) => throw new NotImplementedException();
    public void SetValue<TTarget, TValue>(TTarget target, TValue value) where TTarget : class => throw new NotImplementedException();
}