[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Utilities.Net.Profiling")]
namespace BionicCode.Utilities.Net
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Microsoft.CodeAnalysis;

    internal sealed class PropertyData : MemberInfoData
    {
        private string? displayName;
        private string? shortDisplayName;
        private string? fullyQualifiedDisplayName;
        private string? signature;
        private string? shortSignature;
        private string? shortCompactSignature;
        private string? fullyQualifiedSignature;
        private string? fullyQualifiedRuntimeSignature;
        private string? runtimeSignature;
        private string? runtimeShortSignature;
        private string? runtimeShortCompactSignature;
        private SymbolAttributes symbolAttributes;
        private AccessModifier propertyAccessModifier;
        private AccessModifier setAccessorAccessModifier;
        private AccessModifier getAccessorAccessModifier;
        private ParameterList? indexerParameters;
        private TypeData? propertyTypeData;
        private MethodData? getMethodData;
        private MethodData? setMethodData;
        private bool? isStatic;
        private bool? isOverride;
        private bool? isSealed;
        private bool? canWrite;
        private bool? canRead;
        private bool? isInit;
        private bool? _isPublic;
        private bool? _isPrivate;
        private bool? _isAssembly;
        private bool? _isFamily;
        private bool? _isFamilyOrAssembly;
        private bool? _isFamilyAndAssembly;
        private Func<object, object[], object>? getInvocator;
        private Action<object, object, object[]>? setInvocator;
        private string? assemblyName;
        private SymbolComponentInfo? symbolComponentInfo;
        private bool? isSetMethodReadOnly;

        public PropertyData(PropertyInfo propertyInfo, SymbolInfoDataCacheKey symbolInfoDataCacheKey) : base(propertyInfo, symbolInfoDataCacheKey)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            this.PropertyInfo = propertyInfo;
        }

        public PropertyInfo GetPropertyInfo()
          => this.PropertyInfo;

        protected override MemberInfo GetMemberInfo()
          => GetPropertyInfo();

        public object Get(object target, object[] indexerPropertyIndex = null)
        {
            // TODO::Implement fast invocator pattern

            if (this.getInvocator is null)
            {
                InitializeGetInvocator();
            }

            return this.getInvocator.Invoke(target, indexerPropertyIndex);
        }

        public void Set(object target, object value, object[] indexerPropertyIndex = null)
        {
            if (this.setInvocator is null)
            {
                InitializeSetInvocator();
            }

            this.setInvocator.Invoke(target, value, indexerPropertyIndex);
        }

        public Func<object, object[], object> GetGetInvocator()
        {
            if (this.getInvocator is null)
            {
                InitializeGetInvocator();
            }

            return this.getInvocator;
        }

        public Action<object, object, object[]> GetSetInvocator()
        {
            if (this.setInvocator is null)
            {
                InitializeSetInvocator();
            }

            return this.setInvocator;
        }

        private void InitializeGetInvocator()
          => this.getInvocator = (invocationTarget, indexerIndex) => GetPropertyInfo().GetValue(invocationTarget, indexerIndex);

        private void InitializeSetInvocator()
          => this.setInvocator = (invocationTarget, propertyValue, indexerIndex) => GetPropertyInfo().SetValue(invocationTarget, propertyValue, indexerIndex);

        private void GetAccessors()
        {
            (AccessModifier propertyModifier, AccessModifier getMethodModifier, AccessModifier setMethodModifier) = PropertyData.GetPropertyAccessModifier(this.GetMethodData, this.SetMethodData);
            this.propertyAccessModifier = propertyModifier;
            this.setAccessorAccessModifier = setMethodModifier;
            this.getAccessorAccessModifier = getMethodModifier;
        }

        public bool IsIndexer
          => this.IndexerParameters.HasItems;

        public ParameterList IndexerParameters
          => this.indexerParameters ??= ParameterListBuilder.Create(this.PropertyInfo.GetIndexParameters());

        public override AccessModifier AccessModifier
        {
            get
            {
                if (this.propertyAccessModifier is AccessModifier.Undefined)
                {
                    GetAccessors();
                }

                return this.propertyAccessModifier;
            }
        }

        public AccessModifier SetAccessorAccessModifier
        {
            get
            {
                if (this.setAccessorAccessModifier is AccessModifier.Undefined)
                {
                    GetAccessors();
                }

                return this.setAccessorAccessModifier;
            }
        }

        public AccessModifier GetAccessorAccessModifier
        {
            get
            {
                if (this.getAccessorAccessModifier is AccessModifier.Undefined)
                {
                    GetAccessors();
                }

                return this.getAccessorAccessModifier;
            }
        }

        public TypeData PropertyTypeData
          => this.propertyTypeData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetPropertyInfo().PropertyType);

        public PropertyInfo PropertyInfo { get; }

        //public static PropertyData TaskResultPropertyData
        //  => PropertyData._TaskResultPropertyData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeof(Task<>).GetProperty(nameof(Task<object>.Result)));

        //public static PropertyData ValueTaskResultPropertyData
        //  => PropertyData._ValueTaskResultPropertyData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeof(ValueTask<>).GetProperty(nameof(ValueTask<object>.Result)));

        public bool IsSealed
          => this.isSealed ??= (this.CanRead && this.GetMethodData!.IsSealed)
            || (this.CanWrite && this.SetMethodData!.IsSealed);

        public bool CanWrite
          => this.canWrite ??= GetPropertyInfo().CanWrite;

        public bool IsReadOnly
          => !this.CanWrite;

        public bool CanRead
          => this.canRead ??= GetPropertyInfo().CanRead;

        public bool IsInit
          => this.isInit ??= this.CanWrite && PropertyData.IsPropertyInit(this);

        public MethodData? GetMethodData
          => this.getMethodData ??= GetPropertyInfo() is PropertyInfo propertyInfo && propertyInfo.CanRead
                ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo.GetGetMethod(true)!)
                : null;

        public MethodData? SetMethodData
          => this.setMethodData ??= GetPropertyInfo() is PropertyInfo propertyInfo && propertyInfo.CanWrite
            ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo.GetSetMethod(true)!)
            : null;

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = PropertyData.GetAttributesInternal(this))
          : this.symbolAttributes;

        public override SymbolComponentInfo SymbolComponentInfo
          => this.symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

        public override string Signature
          => this.signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string ShortSignature
          => this.shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

        public override string ShortCompactSignature
          => this.shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

        public override string FullyQualifiedSignature
          => this.fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string FullyQualifiedRuntimeSignature
          => this.fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeSignature
          => this.runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortSignature
          => this.runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortCompactSignature
          => this.runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

        public override string DisplayName
          => this.displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string ShortDisplayName
          => this.shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

        public override string FullyQualifiedDisplayName
          => this.fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string AssemblyName
          => this.assemblyName ??= this.DeclaringTypeData.AssemblyName;

        public override bool IsStatic
          => this.isStatic ??= (this.CanRead && this.GetMethodData!.IsStatic)
            || (this.CanWrite && this.SetMethodData!.IsStatic);

        public bool IsSetMethodReadOnly
          => this.isSetMethodReadOnly ??= this.CanWrite && this.SetMethodData!.AttributeData.Any(data => data.AttributeType == typeof(IsReadOnlyAttribute));

        public bool IsOverride
          => this.isOverride ??= (this.CanRead && this.GetMethodData!.IsOverride)
            || (this.CanWrite && this.SetMethodData!.IsOverride);

        public override bool IsPublic
            => this._isPublic ??= this.AccessModifier == AccessModifier.Public;

        public override bool IsPrivate
            => this._isPrivate ??= this.AccessModifier == AccessModifier.Private;

        /// <summary>
        /// Gets a value indicating whether the member has internal accessibility within its assembly.
        /// </summary>
        public override bool IsAssembly
            => this._isAssembly ??= this.AccessModifier == AccessModifier.Internal;

        /// <summary>
        /// Gets a value indicating whether the member is protected and thus accessible only within its own class or by
        /// derived class instances.
        /// </summary>
        public override bool IsFamily
            => this._isFamily ??= this.AccessModifier == AccessModifier.Protected;

        public override bool IsFamilyOrAssembly
            => this._isFamilyOrAssembly ??= this.AccessModifier == AccessModifier.ProtectedInternal;

        public override bool IsFamilyAndAssembly
            => this._isFamilyAndAssembly ??= this.AccessModifier == AccessModifier.PrivateProtected;

        /// <summary>
        /// Determines the set of symbol attributes for the specified property based on its metadata and accessor
        /// methods.
        /// </summary>
        /// <remarks>For performance reasons avoid querying the attributes and prefer reading the particular property or properties.</remarks>
        /// <param name="propertyData">The metadata describing the property for which to retrieve symbol attributes. Cannot be null.</param>
        /// <returns>A bitwise combination of SymbolAttributes values that represent the characteristics of the property, such as
        /// whether it is static, abstract, virtual, an indexer, or has other modifiers.</returns>
        private static SymbolAttributes GetAttributesInternal(PropertyData propertyData)
        {
            SymbolAttributes propertyAttributes = propertyData.IsIndexer
              ? SymbolAttributes.IndexerProperty
              : SymbolAttributes.Property;

            MethodData? accessorData = propertyData.GetMethodData ?? propertyData.SetMethodData;
            if (accessorData is null)
            {
                return SymbolAttributes.Undefined;
            }

            if (!propertyData.CanWrite)
            {
                propertyAttributes |= SymbolAttributes.Final;
            }

            if (propertyData.IsInit)
            {
                propertyAttributes |= SymbolAttributes.Init;
            }

            if (accessorData.IsAbstract)
            {
                propertyAttributes |= SymbolAttributes.Abstract;
            }

            if (propertyData.IsStatic)
            {
                propertyAttributes |= SymbolAttributes.Static;
            }

            if (accessorData.IsVirtual)
            {
                propertyAttributes |= SymbolAttributes.Virtual;
            }

            if (propertyData.IsOverride)
            {
                propertyAttributes |= SymbolAttributes.Override;
            }

            return propertyAttributes;
        }

        private static bool IsPropertyInit(PropertyData propertyData)
        {
            if (propertyData.CanWrite)
            {
                Type[] requiredModifiers = propertyData.SetMethodData!.GetMethodInfo().ReturnParameter.GetRequiredCustomModifiers();
                if (requiredModifiers.Length > 0)
                {
                    return requiredModifiers.FirstOrDefault(type => type == typeof(IsExternalInit)) != default;
                }
            }

            return false;
        }

        private static (AccessModifier PropertyModifier, AccessModifier GetMethodModifier, AccessModifier SetMethodModifier) GetPropertyAccessModifier(MethodData getMethodData, MethodData setMethodData)
        {
            AccessModifier getMethodModifier = getMethodData.AccessModifier;
            AccessModifier setMethodModifier = setMethodData.AccessModifier;

            // Property accessors with the least restriction provides the access modifier for the property.
            AccessModifier propertyAccessModifier = (AccessModifier)System.Math.Min((int)getMethodModifier, (int)setMethodModifier);

            return (propertyAccessModifier, getMethodModifier, setMethodModifier);
        }
    }
}
