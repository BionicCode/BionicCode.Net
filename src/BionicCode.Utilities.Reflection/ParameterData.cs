namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BionicCode.Utilities.Net.Reflection;

/// <summary>
/// Represents metadata and reflection information for a method, constructor, or property parameter, including its
/// type, position, attributes, and default value.
/// </summary>
/// <remarks>This class provides access to various characteristics of a parameter, such as whether it is
/// passed by reference, is optional, or has a default value. It is intended for use in scenarios that require
/// detailed inspection of parameter metadata, such as code analysis, documentation generation, or advanced
/// reflection tasks.<br/>
/// The key is that the metadata is cached to avoid the reflection overhead for successive calls.<br/>
/// Instances of this class are typically created based on a ParameterInfo object from the <see cref="SymbolReflectionInfoCache"/> API.</remarks>
internal sealed class ParameterData : SymbolInfoData
{
    private SymbolAttributes _symbolAttributes;
    private IList<CustomAttributeData>? _attributeData;
    private bool? _isRef;
    private bool? _isRefReadOnly;
    private bool? _isByRef;
    private bool? _isIn;
    private bool? _isOut;
    private bool? _isOptional;
    private bool? _isParams;
    private TypeData? _parameterTypeData;
    private TypeData? _declaringTypeData;
    private ParameterizedMemberData? _member;
    private string? _assemblyName;
    private SymbolComponentInfo? _symbolComponentInfo;
    private object? _defaultValue;
    private ParameterKind? _parameterKind;
    private bool? _isGenericTypeParameter;
    private bool? _isGenericMethodParameter;
    private bool? _isIndexerPropertyParameter;
    private RuntimeTypeHandle? _declaringTypeHandle;
    private RuntimeTypeHandle? _propertyTypeHandle;
    private int? _position;
    private bool? _isIndexerPropertySetterParameter;
    private bool? _isIndexerPropertyGetterParameter;
    private bool? _isPropertySetterParameter;
    private IParameterDataView? _parameterDataView;

    internal ParameterData(SymbolReflectionInfoCacheKeyInternal symbolInfoDataCacheKey)
        : base(symbolInfoDataCacheKey.ParameterDescriptor.ParameterName, SymbolKind.Parameter, symbolInfoDataCacheKey)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(symbolInfoDataCacheKey);

        ParameterInfo = symbolInfoDataCacheKey.ParameterDescriptor.ParameterInfo;
    }

    internal IParameterDataView View => _parameterDataView
        ??= new ParameterDataView(SymbolReflectionInfoCacheKey.CreateForParameter(this));

    internal RuntimeTypeHandle DeclaringTypeHandle => _declaringTypeHandle ??= ParameterInfo.Member.DeclaringType?.TypeHandle ?? throw new NotSupportedException($"The underlying '{typeof(ParameterInfo).FullName}' belongs to a member that does not return a declaring type.");

    internal RuntimeTypeHandle ParameterTypeHandle => _propertyTypeHandle ??= ParameterInfo.ParameterType.TypeHandle;

    /// <summary>
    /// Gets a value indicating whether the current type is passed by reference using the <see langword="ref"/> keyword.
    /// </summary>
    /// <value><see langword="true"/> if the parameter is passed by reference using the <c>ref</c> keyword; otherwise, <see langword="false"/>.</value>
    internal bool IsRef => _isRef ??= IsRefInternal(this);

    /// <summary>
    /// Gets a value indicating whether the current instance is marked as <see langword="ref"/> <see langword="readonly"/>.
    /// </summary>
    /// <value><see langword="true"/> if the parameter is marked as <see langword="ref"/> <see langword="readonly"/>; otherwise, <see langword="false"/>.</value>
    internal bool IsRefReadOnly => _isRefReadOnly ??= IsRefReadOnlyInternal(this);

    /// <summary>
    /// Gets a value indicating whether the parameter is an input parameter (passed by  reference using the <see langword="in"/> keyword).
    /// </summary>
    /// <value><see langword="true"/> if the parameter is an input parameter; otherwise, <see langword="false"/>.</value>
    internal bool IsIn => _isIn ??= IsInParameter(this);

    /// <summary>
    /// Gets a value indicating whether the parameter is an output parameter (passed by reference using the <see langword="out"/> keyword.
    /// </summary>
    /// <value><see langword="true"/> if the parameter is an output parameter; otherwise, <see langword="false"/>.</value>
    internal bool IsOut => _isOut ??= IsOutParameter(this);

    /// <summary>
    /// Gets a value indicating whether the parameter is optional.
    /// </summary>
    /// <value><see langword="true"/> if the parameter is optional i.e. has a default value; otherwise, <see langword="false"/>.</value>
    /// <remarks>This property does not return whether the parameter is decorated with the <see cref="System.Runtime.InteropServices.OptionalAttribute"/>.
    /// It only checks whether the parameter is considered optional by the existence of a default value by reading <see cref="System.Reflection.ParameterInfo.HasDefaultValue"/>.</remarks>
    internal bool IsOptional => _isOptional ??= ParameterInfo.HasDefaultValue;

    /// <summary>
    /// Gets a value indicating whether the parameter type is a generic type parameter.
    /// </summary>
    /// <value><see langword="true"/> if the parameter type is a generic type parameter; otherwise, <see langword="false"/>.</value>
    internal bool IsGenericTypeParameter => _isGenericTypeParameter ??= ParameterTypeData.IsGenericTypeParameter;

    internal bool IsGenericMethodParameter => _isGenericMethodParameter ??= ParameterTypeData.IsGenericMethodParameter;

    /// <summary>
    /// The modifier that indicates how the parameter is passed (e.g., by value, by reference, as an input parameter, or as an output parameter).
    /// </summary>
    internal ParameterKind ParameterKind => _parameterKind ??= IsIn
        ? ParameterKind.In
        : IsOut
            ? ParameterKind.Out
            : IsRefReadOnly
                ? ParameterKind.RefReadOnly
                : IsRef
                    ? ParameterKind.Ref
                    : ParameterKind.Undefined;

    /// <summary>
    /// Gets the default value for the parameter, if one is defined.
    /// </summary>
    /// <value>The default value of the parameter, or <see langword="null"/> if no default value is defined.</value>
    /// <remarks>If the parameter is optional and a default value is specified, this property returns
    /// that value; otherwise, it returns <see langword="null"/>. The value may be of any type, depending on the parameter's
    /// type. Use <see cref="IsOptional"/> to check if a default value is defined before accessing this property.</remarks>
    /// <exception cref="InvalidOperationException">Thrown when no default value is defined for the parameter.</exception>
    internal object? DefaultValue => _defaultValue ??= IsOptional
        ? ParameterInfo.RawDefaultValue
        : throw new InvalidOperationException("No default value defined.");

    /// <summary>
    /// Zero-based index of the parameter in the formal parameter list.
    /// </summary>
    /// <value>The position of the parameter.</value>
    internal int Position => _position ??= ParameterInfo.Position;

    internal bool IsParams => _isParams ??= ParameterInfo.GetCustomAttribute<ParamArrayAttribute>() != null;

    internal ParameterInfo ParameterInfo { get; }

    /// <summary>
    /// Gets the member (method, constructor, or property accessor) that declares this parameter.
    /// </summary>
    /// <remarks>Since only methods can have parameters in .NET IL, this property will always return a method or constructor.
    /// This means tha the original behaviour of the underlying <see cref="ParameterInfo.Member"/> is normalized in that <see cref="ParameterData.MemberData"/>
    /// will not return a property if the parameter was obtained using <see cref="PropertyData.IndexerParameters"/> (or <see cref="PropertyInfo.GetIndexParameters"/>)
    /// unlike the underlying <see cref="ParameterInfo.Member"/> property which would return a <see cref="PropertyInfo"/> in that scenario.<para/>
    /// Instead, the <see cref="ParameterData.MemberData"/> property will always return the property accessor (getter or setter) that actually declares this parameter.<para/>
    /// That being said, for a parameter that was obtained using <see cref="PropertyData.IndexerParameters"/> (or <see cref="PropertyInfo.GetIndexParameters"/>) the association getter vs setter is ambiguous
    /// since the "value" parameter is excluded from the resulting parameter list.
    /// Hence, we can conclude that he current parameter can't be the "value" parameter of an indexer property's setter (<see cref="IsSetterValueParameter"/> will always be <see langword="false"/>).
    /// In this case, and in addition if both property accessors are implemented, <see cref="ParameterData.MemberData"/> will give the getter (if available) precedence over the setter.
    /// Since the indexer parameters are identical for getter and setter, giving the getter precedence is a formal decision.<para/>
    /// For a parameter that was obtained via <see cref="MethodData.Parameters"/> (or <see cref="MethodBase.GetParameters"/>) the association is clear and the correct declaring setter or getter is returned.<para/>
    /// Use <see cref="ParameterData.IsIndexerPropertyParameter"/> and <see cref="ParameterData.IsIndexerPropertyGetterParameter"/> and <see cref="ParameterData.IsIndexerPropertySetterParameter"/>
    /// and <see cref="ParameterData.IsPropertySetterParameter"/> (or alternatively query the returned <see cref="MethodData"/> e.g. <see cref="MethodData.IsIndexerPropertyGetMethod"/>) to know whether the current <see cref="ParameterData"/>
    /// belongs to an indexer property (getter or setter) or the setter of a non-indexer property.<para/>
    /// If parameter association for indexer properties matters, then always obtain parameters directly from the getter or setter method (e.g. <see cref="MethodData.Parameters"/> like <c>PropertyData.PropertySetMethodData.Parameters</c>) and completely avoid <see cref="PropertyData.IndexerParameters"/> (or <see cref="PropertyInfo.GetIndexParameters"/>).</remarks>
    /// <value>The <see cref="ParameterizedMemberData"/> (which is either a <see cref="MethodData"/> or <see cref="ConstructorData"/>) that declares this parameter.</value>
    internal ParameterizedMemberData MemberData => _member ??= ParameterInfo.Member switch
    {
        ConstructorInfo constructorInfo => SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(constructorInfo),

        // If ParameterInfo.Member is a PropertyInfo, it is ALWAYS an indexer property
        // and the current parameter was obtained via PropertyInfo.GetIndexerParameters(). Since the returned parameter list excludes the "value" parameter for the setter,
        // we can't resolve ambiguity whether the parameter belongs to the getter or setter.
        // This case is no longer supported since ParameterData creation involves normalization. The fact we landed here points to a bug in the code.
        PropertyInfo propertyInfo => throw new NotSupportedException($"Parameters obtained via '{typeof(PropertyInfo).ToFullyQualifiedSignatureName}.{nameof(PropertyInfo.GetIndexParameters)}()' are not supported."),

        // The parameter belongs to a method and was obtained via:
        //      * MethodInfo.GetParameters() or
        //      * ConstructorInfo.GetParameters() or
        //      * PropertyInfo.GetGetMethod().GetParameters() or
        //      * PropertyInfo.GetSetMethod().GetParameters() (which, opposed to PropertyInfo.GetIndexerParameters(), includes the "value" parameter of the property setter).
        MethodInfo methodInfo => SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo),
        _ => throw new NotImplementedException(),
    };

    internal TypeData ParameterTypeData => _parameterTypeData ??= SymbolReflectionInfoCache.GetOrCreateEntryInternal(ParameterInfo.ParameterType);

    internal TypeData DeclaringTypeData => _declaringTypeData ??= MemberData.DeclaringTypeData;

    internal override IList<CustomAttributeData> AttributeData => _attributeData ??= [.. ParameterInfo.GetCustomAttributesData()];

    /// <summary>
    /// Gets a value indicating whether the parameter is passed by reference.
    /// </summary>
    internal bool IsByRef => _isByRef ??= ParameterTypeData.Type.IsByRef;

    internal bool IsIndexerPropertyParameter => _isIndexerPropertyParameter ??= MemberData is MethodData methodData && (methodData.IsIndexerPropertyGetMethod || methodData.IsIndexerPropertySetMethod);

    /// <summary>
    /// Gets a value indicating whether this parameter belongs to the setter method of an indexer property.
    /// </summary>
    /// <value><see langword="true"/> if the parameter belongs to an indexer property setter; otherwise, <see langword="false"/>.<para/>
    /// This value exclusively describes indexer properties and therefore also returns <see langword="false"/> for non-indexer property setters.</value>
    internal bool IsIndexerPropertySetterParameter => _isIndexerPropertySetterParameter ??= MemberData is MethodData methodData && methodData.IsIndexerPropertySetMethod;

    /// <summary>
    /// Returns <see langword="true"/> if the parameter belongs to an indexer property getter.
    /// </summary>
    /// <remarks>This property is typically used to identify parameters that are part of indexer property getters. </remarks>
    internal bool IsIndexerPropertyGetterParameter => _isIndexerPropertyGetterParameter ??= MemberData is MethodData methodData && methodData.IsIndexerPropertyGetMethod;

    /// <summary>
    /// Gets a value indicating whether the parameter represents the implicit 'value' parameter of a property or
    /// indexer setter.
    /// </summary>
    /// <remarks>This property is typically used to identify the parameter that receives the value
    /// being assigned in a property or indexer set accessor. In C#, property and indexer setters include a
    /// compiler-generated parameter named 'value' as the last parameter in the method signature.
    /// </remarks>
    /// <value><see langword="true"/> if the parameter is the implicit 'value' parameter of a property or indexer setter; otherwise, <see langword="false"/>.</value>
    internal bool IsSetterValueParameter => IsPropertySetterParameter // No-indexer property setter always has a single "value" parameter
        || (Name.Equals("value", StringComparison.Ordinal) // "value" parameter is always named "value"
            && Position == MemberData.Parameters.Count - 1 // Calling MethodInfo.GetParameters() on a property setter includes the compiler generated "value" parameter at the end of the parameter list
            && IsIndexerPropertySetterParameter); // Execute most expensive check last

    /// <summary>
    /// Gets a value indicating whether this parameter belongs to a property setter method.<br/>
    /// Note: This property also returns <see langword="false"/> for indexer property setters.
    /// </summary>
    /// <value><see langword="true"/> if the parameter belongs to a non-indexer property setter method; otherwise, <see langword="false"/>.<para/>
    /// This value exclusively describes non-indexer properties and therefore also returns <see langword="false"/> for indexer property setters.</value>
    internal bool IsPropertySetterParameter => _isPropertySetterParameter ??= MemberData is MethodData methodData && methodData.IsPropertySetMethod;

    internal override SymbolAttributes SymbolAttributes => _symbolAttributes is SymbolAttributes.Undefined
      ? (_symbolAttributes = ParameterData.GetAttributesInternal(this))
      : _symbolAttributes;

    internal override SymbolComponentInfo SymbolComponentInfo => _symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

    internal override string Signature => Name;

    internal override string ShortSignature => Name;

    internal override string ShortCompactSignature => Name;

    internal override string FullyQualifiedSignature => Name;

    internal override string FullyQualifiedRuntimeSignature => Name;

    internal override string RuntimeSignature => Name;

    internal override string RuntimeShortSignature => Name;

    internal override string RuntimeShortCompactSignature => Name;

    internal override string DisplayName => Name;

    internal override string ShortDisplayName => Name;

    internal override string FullyQualifiedDisplayName => Name;

    internal override string AssemblyName => _assemblyName ??= MemberData.AssemblyName;

    internal override string Namespace => string.Empty;

    /// <summary>
    /// Determine whether the parameter is passed by reference using the <see langword="ref"/> keyword.
    /// </summary>
    /// <param name="parameterData"></param>
    /// <returns></returns>
    private static bool IsRefInternal(ParameterData parameterData)
    {
        if (!parameterData.IsByRef || parameterData.IsOut)
        {
            return false;
        }

        // No readonly markers → plain ref
        ParameterInfo parameterInfo = parameterData.ParameterInfo;
        return parameterInfo.GetCustomAttribute<IsReadOnlyAttribute>() is null
            && parameterInfo.GetCustomAttribute<RequiresLocationAttribute>() is null;
    }

    private static readonly string s_isReadOnlyAttributeFullName = typeof(IsReadOnlyAttribute).FullName ?? string.Empty;
    private static readonly string s_requiresLocationAttributeFullName = typeof(RequiresLocationAttribute).FullName ?? string.Empty;
    private static readonly string s_inAttributeFullName = typeof(InAttribute).FullName ?? string.Empty;

    /// <summary>
    /// Determine whether the parameter is passed by reference using the <see langword="ref"/> <see langword="readonly"/> keywords.
    /// </summary>
    /// <param name="parameterData"></param>
    /// <returns></returns>
    private static bool IsRefReadOnlyInternal(ParameterData parameterData)
    {
        if (!parameterData.IsByRef || parameterData.IsOut)
        {
            return false;
        }

        bool isMethodReturnParameter = parameterData.MemberData is MethodData methodData && ReferenceEquals(methodData.ReturnParameterData, parameterData);
        if (isMethodReturnParameter)
        {
            return HasOptionalCustomModifier(parameterData, s_inAttributeFullName)
                || HasAttribute<IsReadOnlyAttribute>(parameterData, s_inAttributeFullName);
        }

        // No readonly markers → plain ref readonly
        return HasAttribute<RequiresLocationAttribute>(parameterData, s_requiresLocationAttributeFullName) // For normal parameters
            || HasOptionalCustomModifier(parameterData, s_requiresLocationAttributeFullName); // Support fnptr modopt cases
    }

    private static bool HasAttribute<TAttribute>(ParameterData parameterData, string attributeName) where TAttribute : Attribute => parameterData.ParameterInfo.IsDefined(typeof(TAttribute), inherit: false)
        || parameterData.ParameterInfo.GetCustomAttributesData().Any(attribute => attribute.AttributeType.FullName?.Equals(attributeName, StringComparison.Ordinal) ?? false);

    private static bool HasOptionalCustomModifier(ParameterData parameterData, string modifierName) => parameterData.ParameterInfo.GetOptionalCustomModifiers().Any(modifier => modifier.FullName?.Equals(modifierName, StringComparison.Ordinal) ?? false);

    private static bool IsOutParameter(ParameterData parameterData) => parameterData.IsByRef && parameterData.ParameterInfo.IsOut;

    private static bool IsInParameter(ParameterData parameterData)
    {
        if (!parameterData.ParameterTypeData.IsByRef || parameterData.IsOut)
        {
            return false;
        }

        // C# 'in' → IsReadOnlyAttribute, but not ref readonly
        bool isMarkedReadOnly = HasAttribute<IsReadOnlyAttribute>(parameterData, s_isReadOnlyAttributeFullName)
            && !IsRefReadOnlyInternal(parameterData);

        return isMarkedReadOnly;
    }

    /// <summary>
    /// Determines the set of symbol attributes for a parameter based on its metadata.
    /// </summary>
    /// <remarks>For performance reasons avoid querying the attributes and prefer reading the particular property or properties.</remarks>
    /// <param name="parameterData">The metadata describing the parameter, including its direction and optionality.</param>
    /// <returns>A bitwise combination of SymbolAttributes flags that represent the parameter's characteristics, such as In,
    /// Out, Ref, and Optional.</returns>
    private static SymbolAttributes GetAttributesInternal(ParameterData parameterData)
    {
        SymbolAttributes parameterAttributes = SymbolAttributes.Parameter;
        if (parameterData.IsIn)
        {
            parameterAttributes |= SymbolAttributes.InParameter;
        }

        if (parameterData.IsRef)
        {
            parameterAttributes |= SymbolAttributes.RefParameter;
        }

        if (parameterData.IsOut)
        {
            parameterAttributes |= SymbolAttributes.OutParameter;
        }

        if (parameterData.IsOptional)
        {
            parameterAttributes |= SymbolAttributes.OptionalParameter;
        }

        return parameterAttributes;
    }
}
