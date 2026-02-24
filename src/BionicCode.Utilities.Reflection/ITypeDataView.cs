namespace BionicCode.Utilities.Net.Reflection;

using System.Reflection;
using BionicCode.Utilities.Net;

public interface ITypeDataView : ISymbolInfoDataView
{
    AccessModifier AccessModifier { get; }
    ITypeDataView? BaseTypeData { get; }
    bool CanDeclareExtensionMethod { get; }
    SymbolComponentInfo CompactSymbolComponentInfo { get; }
    IConstructorListView Constructors { get; }
    bool ContainsGenericParameters { get; }
    IMethodDataView DelegateInvokeMethodData { get; }
    IEventListView Events { get; }
    IEventListView ExplicitInterfaceEvents { get; }
    IMethodListView ExplicitInterfaceMethods { get; }
    IPropertyListView ExplicitInterfaceProperties { get; }
    IFieldListView Fields { get; }
    GenericParameterAttributes GenericParameterAttributes { get; }
    ITypeListView GenericParameterConstraintsData { get; }
    ITypeListView GenericTypeArguments { get; }
    ITypeDataView GenericTypeDefinitionData { get; }
    RuntimeTypeHandle Handle { get; }
    ITypeListView InterfacesData { get; }
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

    IEnumerable<ConstructorData> EnumerateConstructors(BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
    IEnumerable<EventData> EnumerateEvents(BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
    IEnumerable<FieldData> EnumerateFields(BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
    IEnumerable<MethodData> EnumerateMethods(BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
    IEnumerable<PropertyData> EnumerateProperties(BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
    bool TryGetConstructorByParameterList(ParameterDescriptorList parameters, out ConstructorData? constructorData);
    bool TryGetConstructorByParameterList(ParameterList parameters, out ConstructorData? constructorData);
    bool TryGetEventByName(string eventName, out EventData? eventData);
    bool TryGetExplicitInterfaceEvent(RuntimeTypeHandle declaringInterfaceTypeHandle, string eventName, out EventData? eventData);
    bool TryGetExplicitInterfaceIndexerPropertyByParameterList(ParameterDescriptorList indexerParameters, PropertyAccessors indexerPropertyAccessor, out PropertyData? propertyData);
    bool TryGetExplicitInterfaceIndexerPropertyByParameterList(ParameterList parameters, PropertyAccessors propertyAccessor, out PropertyData? propertyData);
    bool TryGetExplicitInterfaceMethod(string methodName, TypeList genericMethodParameters, ParameterDescriptorList parameters, out MethodData? methodData);
    bool TryGetExplicitInterfaceMethod(string methodName, TypeList genericMethodParameters, ParameterList parameters, out MethodData? methodData);
    bool TryGetExplicitInterfacePropertyByName(string propertyName, out PropertyData? propertyData);
    bool TryGetFieldByName(string fieldName, out FieldData? fieldData);
    bool TryGetIndexerPropertyByParameterList(ParameterDescriptorList indexerParameters, PropertyAccessors indexerPropertyAccessor, out PropertyData? propertyData);
    bool TryGetIndexerPropertyByParameterList(ParameterList parameters, PropertyAccessors propertyAccessor, out PropertyData? propertyData);
    bool TryGetMethod(string methodName, TypeList genericMethodParameters, ParameterDescriptorList parameters, out MethodData? methodData);
    bool TryGetMethod(string methodName, TypeList genericMethodParameters, ParameterList parameters, out MethodData? methodData);
    bool TryGetMethodByName(string methodName, out MethodList? methods);
    bool TryGetPropertyByName(string propertyName, out PropertyData? propertyData);
    object? InvokeDelegate(object? target, params object?[]? arguments);
    TResult InvokeDelegate<TTarget, TResult>(TTarget target, params object?[]? arguments);
}