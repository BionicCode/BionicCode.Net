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
    /// The key is that the metadata is cached to aboid the reflection overhead for successive calls.<br/>
    /// Instances of this class are typically created based on a ParameterInfo object from the <see cref="SymbolReflectionInfoCache"/> API.</remarks>
    internal sealed class ParameterData : SymbolInfoData
    {
        private SymbolAttributes symbolAttributes;
        private IList<CustomAttributeData> attributeData;
        private bool? isRef;
        private bool? isRefReadonly;
        private bool? isByRef;
        private bool? isIn;
        private bool? isOut;
        private bool? isOptional;
        private bool? isParams;
        private int? position;
        private TypeData parameterTypeData;
        private TypeData declaringTypeData;
        private MemberInfoData member;
        private string assemblyName;
        private SymbolComponentInfo symbolComponentInfo;
        private object? defaultValue;

        public ParameterData(ParameterInfo parameterInfo) : base(parameterInfo.Name)
        {
            this.DeclaringTypeHandle = parameterInfo.Member.DeclaringType.TypeHandle;
            this.ParameterInfo = parameterInfo;
        }

        public ParameterInfo GetParameterInfo()
          => this.ParameterInfo;

        public Type GetDeclaringType()
          => Type.GetTypeFromHandle(this.DeclaringTypeHandle);

        public RuntimeTypeHandle DeclaringTypeHandle { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current type is passed by reference using the <see langword="ref"/> keyword.
        /// </summary>
        /// <value><c>true</c> if the parameter is passed by reference using the <c>ref</c> keyword; otherwise, <c>false</c>.</value>
        public bool IsRef
          => this.isRef ??= IsRefInternal(this);

        /// <summary>
        /// Gets a value indicating whether the current instance is marked as <see langword="ref"/> <see langword="readonly"/>.
        /// </summary>
        /// <value><c>true</c> if the parameter is marked as <see langword="ref"/> <see langword="readonly"/>; otherwise, <c>false</c>.</value>
        public bool IsRefReadonly
          => this.isRefReadonly ??= IsRefReadonlyInternal(this);

        /// <summary>
        /// Gets a value indicating whether the parameter is an input parameter (passed by  reference using the <see langword="in"/> keyword).
        /// </summary>
        /// <value><c>true</c> if the parameter is an input parameter; otherwise, <c>false</c>.</value>
        public bool IsIn
          => this.isIn ??= IsInParameter(this);

        /// <summary>
        /// Gets a value indicating whether the parameter is an output parameter (passed by reference using the <see langword="out"/> keyword.
        /// </summary>
        /// <value><c>true</c> if the parameter is an output parameter; otherwise, <c>false</c>.</value>
        public bool IsOut
          => this.isOut ??= IsOutParameter(this);

        /// <summary>
        /// Gets a value indicating whether the parameter is optional.
        /// </summary>
        /// <value><c>true</c> if the parameter is optional i.e. has a default value; otherwise, <c>false</c>.</value>
        /// <remarks>This property does not return whether the parameter is decorated with  the <c>System.Runtime.InteropServices.OptionalAttribuute</c>. It only checks whether the parameter is considered optional by the existance of a default value.</remarks>
        public bool IsOptional
          => this.isOptional ??= GetParameterInfo().HasDefaultValue;

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
          => this.position ??= GetParameterInfo().Position;

        public bool IsParams
          => this.isParams ??= GetParameterInfo().GetCustomAttribute<ParamArrayAttribute>() != null;

        public ParameterInfo ParameterInfo { get; }

        public MemberInfoData Member
        {
            get
            {
                MemberInfo member = GetParameterInfo().Member;
                if (this.member is null)
                {
                    if (member is ConstructorInfo constructorInfo)
                    {
                        this.member = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
                    }
                    else if (member is PropertyInfo propertyInfo)
                    {
                        this.member = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
                    }
                    else if (member is MethodInfo methodInfo)
                    {
                        this.member = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                return this.member;
            }
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
          => this.isByRef ??= this.ParameterTypeData.GetType().IsByRef;

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
          => this.assemblyName ??= this.Member.AssemblyName;

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

        internal static bool IsRefReadonlyInternal(ParameterData parameterData)
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
