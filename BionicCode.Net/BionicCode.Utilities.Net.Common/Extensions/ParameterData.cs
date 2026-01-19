namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;

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
        private SymbolAttributes symbolAttributes;
        private IList<CustomAttributeData>? attributeData;
        private bool? isRef;
        private bool? isRefReadOnly;
        private bool? isByRef;
        private bool? isIn;
        private bool? isOut;
        private bool? isOptional;
        private bool? isParams;
        private TypeData? parameterTypeData;
        private TypeData? declaringTypeData;
        private ParameterizedMemberData? member;
        private string? assemblyName;
        private SymbolComponentInfo? symbolComponentInfo;
        private object? defaultValue;
        private ParameterKind? parameterKind;
        private bool? isGenericTypeParameter;
        private bool? isGenericMethodParameter;
        private bool? _isIndexerPropertyParameter;
        private RuntimeTypeHandle? _declaringTypeHandle;
        private RuntimeTypeHandle? _propertyTypeHandle;
        private int? _position;
        private bool? _isIndexerPropertySetterParameter;
        private bool? _isIndexerPropertyGetterParameter;
        private bool? _isPropertySetterParameter;
        private bool? _isIndexerAccessorAmbiguous;

        public ParameterData(ParameterInfo parameterInfo, SymbolInfoDataCacheKey symbolInfoDataCacheKey) : base(parameterInfo.Name, SymbolKind.Parameter, symbolInfoDataCacheKey)
        {
            ArgumentNullException.ThrowIfNull(parameterInfo, nameof(parameterInfo));

            this.ParameterInfo = parameterInfo;
        }

        public ParameterInfo GetParameterInfo()
          => this.ParameterInfo;

        public Type GetDeclaringType()
          => Type.GetTypeFromHandle(this.DeclaringTypeHandle)!;

        public RuntimeTypeHandle DeclaringTypeHandle
            => this._declaringTypeHandle ??= GetParameterInfo().Member.DeclaringType?.TypeHandle ?? throw new NotSupportedException($"The underlying '{typeof(ParameterInfo).FullName}' belongs to a member that does not return a declaring type.");

        public RuntimeTypeHandle ParameterTypeHandle
            => this._propertyTypeHandle ??= GetParameterInfo().ParameterType.TypeHandle;

        /// <summary>
        /// Gets a value indicating whether the current type is passed by reference using the <see langword="ref"/> keyword.
        /// </summary>
        /// <value><see langword="true"/> if the parameter is passed by reference using the <c>ref</c> keyword; otherwise, <see langword="false"/>.</value>
        public bool IsRef
          => this.isRef ??= IsRefInternal(this);

        /// <summary>
        /// Gets a value indicating whether the current instance is marked as <see langword="ref"/> <see langword="readonly"/>.
        /// </summary>
        /// <value><see langword="true"/> if the parameter is marked as <see langword="ref"/> <see langword="readonly"/>; otherwise, <see langword="false"/>.</value>
        public bool IsRefReadOnly
          => this.isRefReadOnly ??= IsRefReadOnlyInternal(this);

        /// <summary>
        /// Gets a value indicating whether the parameter is an input parameter (passed by  reference using the <see langword="in"/> keyword).
        /// </summary>
        /// <value><see langword="true"/> if the parameter is an input parameter; otherwise, <see langword="false"/>.</value>
        public bool IsIn
          => this.isIn ??= IsInParameter(this);

        /// <summary>
        /// Gets a value indicating whether the parameter is an output parameter (passed by reference using the <see langword="out"/> keyword.
        /// </summary>
        /// <value><see langword="true"/> if the parameter is an output parameter; otherwise, <see langword="false"/>.</value>
        public bool IsOut
          => this.isOut ??= IsOutParameter(this);

        /// <summary>
        /// Gets a value indicating whether the parameter is optional.
        /// </summary>
        /// <value><see langword="true"/> if the parameter is optional i.e. has a default value; otherwise, <see langword="false"/>.</value>
        /// <remarks>This property does not return whether the parameter is decorated with  the <c>System.Runtime.InteropServices.OptionalAttribuute</c>. It only checks whether the parameter is considered optional by the existance of a default value.</remarks>
        public bool IsOptional
          => this.isOptional ??= GetParameterInfo().HasDefaultValue;

        /// <summary>
        /// Gets a value indicating whether the parameter type is a generic type parameter.
        /// </summary>
        /// <value><see langword="true"/> if the parameter type is a generic type parameter; otherwise, <see langword="false"/>.</value>
        public bool IsGenericTypeParameter
          => this.isGenericTypeParameter ??= this.ParameterTypeData.IsGenericTypeParameter;

        public bool IsGenericMethodParameter
          => this.isGenericMethodParameter ??= this.ParameterTypeData.IsGenericMethodParameter;

        public ParameterKind ParameterKind
          => this.parameterKind ??= this.IsIn ? ParameterKind.In
            : this.IsOut ? ParameterKind.Out
            : this.IsRefReadOnly ? ParameterKind.RefReadOnly
            : this.IsRef ? ParameterKind.Ref
            : ParameterKind.Undefined;

        /// <summary>
        /// Gets the default value for the parameter, if one is defined.
        /// </summary>
        /// <value>The default value of the parameter, or null if no default value is defined.</value>
        /// <remarks>If the parameter is optional and a default value is specified, this property returns
        /// that value; otherwise, it returns null. The value may be of any type, depending on the parameter's
        /// type.</remarks>
        public object? DefaultValue
            => this.IsOptional && this.defaultValue is null ? (this.defaultValue = GetParameterInfo().RawDefaultValue) : default;

        /// <summary>
        /// Zero-based index of the parameter in the formal parameter list.
        /// </summary>
        /// <value>The position of the parameter.</value>
        public int Position
            => this._position ??= GetParameterInfo().Position;

        public bool IsParams
          => this.isParams ??= GetParameterInfo().GetCustomAttribute<ParamArrayAttribute>() != null;

        public ParameterInfo ParameterInfo { get; }

        /// <summary>
        /// Gets the member (method, constructor, or property accessor) that declares this parameter.
        /// </summary>
        /// <remarks>Since only methods can have parameters in .NET IL, this property will always return a method or constructor.
        /// This means tha the original behaviour of the underlying <see cref="ParameterInfo.Member"/> is normalized in that <see cref="ParameterData.MemberData"/> will not return a property if the parameter was obtained using <see cref="PropertyData.IndexerParameters"/> (or <see cref="PropertyInfo.GetIndexParameters"/>).<br/>
        /// Instead, the <see cref="ParameterData.MemberData"/> property will always return the property accessor (getter or setter) that actually declares this parameter.<para/>
        /// That being said, for a parameter that was obtained using <see cref="PropertyData.IndexerParameters"/> (or <see cref="PropertyInfo.GetIndexParameters"/>) the association getter vs setter is ambiguous since the "value" parameter is removed from the resulting parameter list.
        /// In this case, <see cref="ParameterData.MemberData"/> will give the getter (if available) precedence over the setter. For a parameter that was obtained via <see cref="MethodData.Parameters"/> (or <see cref="MethodBase.GetParameters"/>) the association is clear and the correct declaring setter or getter is returned.<para/>
        /// Use <see cref="ParameterData.IsIndexerPropertyParameter"/> and <see cref="ParameterData.IsIndexerPropertyGetterParameter"/> and <see cref="ParameterData.IsIndexerPropertySetterParameter"/> and <see cref="ParameterData.IsPropertySetterParameter"/> (or alternatively query the returned <see cref="MethodData"/> e.g. <see cref="MethodData.IsIndexerPropertyGetMethod"/>) to know whether the current <see cref="ParameterData"/>
        /// belongs to an indexer property (getter or setter) or the setter of a non-indexer property.<para/>
        /// If parameter association for indexer properties matters, then always obtain parameters directly from the getter or setter method (e.g. <see cref="MethodData.Parameters"/> like <c>PropertyData.PropertySetMethodData.Parameters</c>) and completely avoid <see cref="PropertyData.IndexerParameters"/> (or <see cref="PropertyInfo.GetIndexParameters"/>).</remarks>
        /// <value>The <see cref="ParameterizedMemberData"/> (which is either a <see cref="MethodData"/> or <see cref="ConstructorData"/>) that declares this parameter.</value>
        public ParameterizedMemberData MemberData
            => this.member ??= GetParameterInfo().Member switch
            {
                ConstructorInfo constructorInfo => SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo),

                // If ParameterInfo.Member is a PropertyInfo, it is ALWAYS an indexer property
                // and the current parameter was obtained via PropertyInfo.GetIndexerParameters(). Since the returned parameter list excludes the "value" parameter for the setter,
                // we can't resolve ambiguity whether the parameter belongs to the getter or setter.
                PropertyInfo propertyInfo => FindDeclaringPropertyAccessor(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo)),

                // The parameter belongs to a method and was obtained via:
                //      * MethodInfo.GetParameters() or
                //      * ConstructorInfo.GetParameters() or
                //      * PropertyInfo.GetGetMethod().GetParameters() or
                //      * PropertyInfo.GetSetMethod().GetParameters() (which, opposed to PropertyInfo.GetIndexerParameters(), includes the "value" parameter of the property setter).
                MethodInfo methodInfo => SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo),
                _ => throw new NotImplementedException(),
            };

        /// <summary>
        /// Assumes that the parameter belongs to an indexer property accessor and was obtained via
        /// PropertyInfo.GetIndexerParameters().
        /// </summary>
        /// <param name="propertyData"></param>
        /// <returns></returns>
        internal MethodData FindDeclaringPropertyAccessor(PropertyData propertyData)
        {
            this._isIndexerAccessorAmbiguous = true;

            return propertyData.CanRead
                ? propertyData.PropertyGetMethodData
                : propertyData.PropertySetMethodData;
        }

        public TypeData ParameterTypeData
          => this.parameterTypeData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetParameterInfo().ParameterType);

        public TypeData DeclaringTypeData
          => this.declaringTypeData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetDeclaringType());

        public override IList<CustomAttributeData> AttributeData
          => this.attributeData ??= new List<CustomAttributeData>(GetParameterInfo().GetCustomAttributesData());

        /// <summary>
        /// Gets a value indicating whether the parameter is passed by reference.
        /// </summary>
        internal bool IsByRef
          => this.isByRef ??= this.ParameterTypeData.UnwrapType().IsByRef;

        public bool IsIndexerPropertyParameter
          => this._isIndexerPropertyParameter ??= this.MemberData is MethodData methodData && (methodData.IsIndexerPropertyGetMethod || methodData.IsIndexerPropertySetMethod);

        public bool IsIndexerPropertySetterParameter
          => this._isIndexerPropertySetterParameter ??= this.MemberData is MethodData methodData && methodData.IsIndexerPropertySetMethod;

        public bool IsIndexerPropertyGetterParameter
          => this._isIndexerPropertyGetterParameter ??= this.MemberData is MethodData methodData && methodData.IsIndexerPropertyGetMethod;

        public bool IsIndexerAccessorAmbiguous
            => !this.IsIndexerPropertyParameter && (bool)this._isIndexerAccessorAmbiguous!; // Accessing 'IsIndexerPropertyParameter' will set the ambiguity flag

        /// <summary>
        /// Gets a value indicating whether this parameter belongs to a property setter method.<br/>
        /// Note: This property also returns <see langword="false"/> for indexer property setter
        /// </summary>
        public bool IsPropertySetterParameter
          => this._isPropertySetterParameter ??= this.MemberData is MethodData methodData && methodData.IsPropertySetMethod;

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = ParameterData.GetAttributesInternal(this))
          : this.symbolAttributes;

        public override SymbolComponentInfo SymbolComponentInfo
          => this.symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

        public override string Signature
          => this.Name;

        public override string ShortSignature
          => this.Name;

        public override string ShortCompactSignature
          => this.Name;

        public override string FullyQualifiedSignature
          => this.Name;

        public override string FullyQualifiedRuntimeSignature
          => this.Name;

        public override string RuntimeSignature
          => this.Name;

        public override string RuntimeShortSignature
          => this.Name;

        public override string RuntimeShortCompactSignature
          => this.Name;

        public override string DisplayName
          => this.Name;

        public override string ShortDisplayName
          => this.Name;

        public override string FullyQualifiedDisplayName
          => this.Name;

        public override string AssemblyName
          => this.assemblyName ??= this.MemberData.AssemblyName;

        internal static bool IsRefInternal(ParameterData parameterData)
        {
            if (!parameterData.IsByRef || parameterData.IsOut)
            {
                return false;
            }

            // No readonly markers → plain ref
            ParameterInfo parameterInfo = parameterData.GetParameterInfo();
            return parameterInfo.GetCustomAttribute<IsReadOnlyAttribute>() is null
                && parameterInfo.GetCustomAttribute<RequiresLocationAttribute>() is null;
        }

        internal static bool IsRefReadOnlyInternal(ParameterData parameterData)
        {
            if (!parameterData.IsByRef || parameterData.IsOut)
            {
                return false;
            }

            // No readonly markers → plain ref
            ParameterInfo parameterInfo = parameterData.GetParameterInfo();
            return parameterInfo.GetCustomAttribute<RequiresLocationAttribute>() is not null;
        }

        private static bool IsOutParameter(ParameterData parameterData)
          => parameterData.IsByRef && parameterData.IsOut;

        private static bool IsInParameter(ParameterData parameterData)
        {
            if (!parameterData.ParameterTypeData.IsByRef || parameterData.IsOut)
            {
                return false;
            }

            // C# 'in' → IsReadOnlyAttribute, but not ref readonly
            ParameterInfo parameterInfo = parameterData.GetParameterInfo();
            bool hasReadOnly = parameterInfo.GetCustomAttribute<IsReadOnlyAttribute>() is not null;
            bool hasReqLoc = parameterInfo.GetCustomAttribute<RequiresLocationAttribute>() is not null;

            return hasReadOnly && !hasReqLoc;
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
}
