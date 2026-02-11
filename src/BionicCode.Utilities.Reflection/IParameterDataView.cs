namespace BionicCode.Utilities.Net.Reflection;

public interface IParameterDataView : ISymbolInfoDataView
{
    ITypeDataView DeclaringTypeData { get; }
    RuntimeTypeHandle DeclaringTypeHandle { get; }
    object? DefaultValue { get; }
    bool IsByRef { get; }
    bool IsGenericMethodParameter { get; }
    bool IsGenericTypeParameter { get; }
    bool IsIn { get; }
    bool IsIndexerPropertyGetterParameter { get; }
    bool IsIndexerPropertyParameter { get; }
    bool IsIndexerPropertySetterParameter { get; }
    bool IsOptional { get; }
    bool IsOut { get; }
    bool IsParams { get; }
    bool IsPropertySetterParameter { get; }
    bool IsRef { get; }
    bool IsRefReadOnly { get; }
    bool IsSetterValueParameter { get; }
    IParameterizedMemberDataView MemberData { get; }
    ParameterKind ParameterKind { get; }
    ITypeDataView ParameterTypeData { get; }
    RuntimeTypeHandle ParameterTypeHandle { get; }
    int Position { get; }
}