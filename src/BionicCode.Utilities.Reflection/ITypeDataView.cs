namespace BionicCode.Utilities.Net.Reflection;

using System.Reflection;
using BionicCode.Utilities.Net;

public interface ITypeDataView : ISymbolInfoDataView
{
    AccessModifier AccessModifier { get; }
    ITypeDataView? BaseType { get; }
    bool CanDeclareExtensionMethod { get; }
    SymbolComponentInfo CompactSymbolComponentInfo { get; }
    IConstructorListView Constructors { get; }
    bool ContainsGenericParameters { get; }
    IMethodDataView DelegateInvokeMethod { get; }
    IEventListView Events { get; }
    IEventListView ExplicitInterfaceEvents { get; }
    IMethodListView ExplicitInterfaceMethods { get; }
    IPropertyListView ExplicitInterfaceProperties { get; }
    IFieldListView Fields { get; }
    GenericParameterAttributes GenericParameterAttributes { get; }
    ITypeListView GenericParameterConstraints { get; }
    ITypeListView GenericTypeArguments { get; }
    ITypeDataView GenericTypeDefinition { get; }
    RuntimeTypeHandle Handle { get; }
    ITypeListView Interfaces { get; }
    bool IsAbstract { get; }
    bool IsAwaitable { get; }
    bool IsAwaitableTask { get; }
    bool IsAwaitableValueTask { get; }
    bool IsBuiltInType { get; }
    bool IsByRef { get; }
    bool IsByRefLike { get; }
    bool IsClass { get; }
    bool IsDelegate { get; }
    bool IsEnum { get; }
    bool IsGenericMethodParameter { get; }
    bool IsGenericParameter { get; }
    bool IsGenericType { get; }
    bool IsGenericTypeDefinition { get; }
    bool IsGenericTypeParameter { get; }
    bool IsInterface { get; }
    bool IsNestedAssembly { get; }
    bool IsNestedFamANDAssem { get; }
    bool IsNestedFamily { get; }
    bool IsNestedFamORAssem { get; }
    bool IsNestedPrivate { get; }
    bool IsNestedPublic { get; }
    bool IsPublic { get; }
    bool IsReadOnlyStruct { get; }
    bool IsReferenceType { get; }
    bool IsSealed { get; }
    bool IsStatic { get; }
    bool IsStruct { get; }
    bool IsSubclass { get; }
    bool IsValueType { get; }
    bool IsVisible { get; }
    IMethodListView Methods { get; }
    IPropertyListView Properties { get; }

    IEnumerable<IConstructorDataView> EnumerateConstructors(MemberEnumerationRule enumerationRule);
    IEnumerable<IEventDataView> EnumerateEvents(MemberEnumerationRule enumerationRule);
    IEnumerable<IFieldDataView> EnumerateFields(MemberEnumerationRule enumerationRule);
    IEnumerable<IMethodDataView> EnumerateMethods(MemberEnumerationRule enumerationRule);
    IEnumerable<IPropertyDataView> EnumerateProperties(MemberEnumerationRule enumerationRule);
    bool TryGetConstructorByParameterList(ParameterDescriptorList parameters, out IConstructorDataView? constructorData);
    bool TryGetEventByName(string eventName, out IEventDataView? eventData);
    bool TryGetExplicitInterfaceEvent(RuntimeTypeHandle declaringInterfaceTypeHandle, string eventName, out IEventDataView? eventData);
    bool TryGetExplicitInterfaceIndexerPropertyByParameterList(ParameterDescriptorList indexerParameters, PropertyAccessors indexerPropertyAccessor, out IPropertyDataView? propertyData);
    bool TryGetExplicitInterfaceMethod(string methodName, ITypeListView? genericMethodParameters, ParameterDescriptorList? parameters, out IMethodDataView? methodData);
    bool TryGetExplicitInterfacePropertyByName(string propertyName, out IPropertyDataView? propertyData);
    bool TryGetFieldByName(string fieldName, out IFieldDataView? fieldData);
    bool TryGetIndexerPropertyByParameterList(ParameterDescriptorList indexerParameters, PropertyAccessors indexerPropertyAccessor, out IPropertyDataView? propertyData);
    bool TryGetMethod(string methodName, ITypeListView? genericMethodParameters, ParameterDescriptorList? parameters, out IMethodDataView? methodData);
    bool TryGetMethodByName(string methodName, out IMethodListView? methods);
    bool TryGetPropertyByName(string propertyName, out IPropertyDataView? propertyData);
    object? InvokeDelegate(object? target, params object?[]? arguments);
    TResult InvokeDelegate<TTarget, TResult>(TTarget target, params object?[]? arguments);
}