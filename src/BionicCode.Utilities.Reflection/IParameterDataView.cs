namespace BionicCode.Utilities.Net.Reflection;

public interface IParameterDataView : ISymbolInfoDataView
{
    bool IsDefined<TAttribute>(bool inherit = false) where TAttribute : Attribute;
    bool IsDefined(Type attributeType, bool inherit = false);
    bool HasOptionalCustomModifier(string modifierName);
    bool HasRequiredCustomModifier(string modifierName);
    ITypeDataView DeclaringType { get; }
    RuntimeTypeHandle DeclaringTypeHandle { get; }
    object? DefaultValue { get; }
    bool IsByRef { get; }
    bool IsDynamic { get; }
    bool IsGeneric { get; }
    bool IsGenericMethodParameter { get; }
    bool IsGenericTypeParameter { get; }
    bool IsIn { get; }
    bool IsMethodReturnParameter { get; }
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
    IParameterizedMemberDataView Member { get; }
    ParameterModifier ParameterModifier { get; }
    ITypeDataView ParameterType { get; }
    RuntimeTypeHandle ParameterTypeHandle { get; }
    int Position { get; }
    ITypeListView RequiredModifiers { get; }
    ITypeListView OptionalModifiers { get; }
}