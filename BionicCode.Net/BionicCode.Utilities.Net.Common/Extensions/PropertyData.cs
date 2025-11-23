[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Utilities.Net.Profiling")]
namespace BionicCode.Utilities.Net
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;

    internal sealed class PropertyData : MemberInfoData
    {
        private static PropertyData _TaskResultPropertyData;
        private static PropertyData _ValueTaskResultPropertyData;
        private string displayName;
        private string shortDisplayName;
        private string fullyQualifiedDisplayName;
        private string signature;
        private string shortSignature;
        private string shortCompactSignature;
        private string fullyQualifiedSignature;
        private string fullyQualifiedRuntimeSignature;
        private string runtimeSignature;
        private string runtimeShortSignature;
        private string runtimeShortCompactSignature;
        private SymbolAttributes symbolAttributes;
        private AccessModifier? propertyAccessModifier;
        private AccessModifier? setAccessorAccessModifier;
        private AccessModifier? getAccessorAccessModifier;
        private ParameterData[] indexerParameters;
        private TypeData propertyTypeData;
        private MethodData getMethodData;
        private MethodData setMethodData;
        private bool? isStatic;
        private bool? isOverride;
        private bool? isSealed;
        private bool? canWrite;
        private bool? canRead;
        private Func<object, object[], object> getInvocator;
        private Action<object, object, object[]> setInvocator;
        private string assemblyName;
        private SymbolComponentInfo symbolComponentInfo;

#if !NETSTANDARD2_0
        private bool? isSetMethodReadOnly;
#endif

        public PropertyData(PropertyInfo propertyInfo) : base(propertyInfo) => this.PropertyInfo = propertyInfo;

        public PropertyInfo GetPropertyInfo()
          => this.PropertyInfo;

        protected override MemberInfo GetMemberInfo()
          => GetPropertyInfo();

        public object Get(object target, object[] indexerPropertyIndex = null)
        {
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
            (AccessModifier propertyModifier, AccessModifier getMethodModifier, AccessModifier setMethodModifier) = HelperExtensionsCommon.GetPropertyAccessModifier(this.GetMethodData, this.SetMethodData);
            this.propertyAccessModifier = propertyModifier;
            this.setAccessorAccessModifier = setMethodModifier;
            this.getAccessorAccessModifier = getMethodModifier;
        }

        public bool IsIndexer
          => this.IndexerParameters.Length > 0;

        public ParameterData[] IndexerParameters
          => this.indexerParameters ??= this.PropertyInfo.GetIndexParameters().Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray();

        public override AccessModifier AccessModifier
        {
            get
            {
                if (this.propertyAccessModifier is null)
                {
                    GetAccessors();
                }

                return this.propertyAccessModifier.Value;
            }
        }

        public AccessModifier SetAccessorAccessModifier
        {
            get
            {
                if (this.setAccessorAccessModifier is null)
                {
                    GetAccessors();
                }

                return this.setAccessorAccessModifier.Value;
            }
        }

        public AccessModifier GetAccessorAccessModifier
        {
            get
            {
                if (this.getAccessorAccessModifier is null)
                {
                    GetAccessors();
                }

                return this.getAccessorAccessModifier.Value;
            }
        }

        public TypeData PropertyTypeData
          => this.propertyTypeData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetPropertyInfo().PropertyType);

        public PropertyInfo PropertyInfo { get; }

        public static PropertyData TaskResultPropertyData
          => PropertyData._TaskResultPropertyData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeof(Task<>).GetProperty(nameof(Task<object>.Result)));

        public static PropertyData ValueTaskResultPropertyData
          => PropertyData._ValueTaskResultPropertyData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeof(ValueTask<>).GetProperty(nameof(ValueTask<object>.Result)));

        public bool IsSealed
          => (bool)(bool?)(this.isSealed ??= this.CanRead ? this.GetMethodData.IsSealed : this.SetMethodData.IsSealed);

        public bool CanWrite
          => (bool)(bool?)(this.canWrite ??= GetPropertyInfo().CanWrite);

        public bool CanRead
          => (bool)(bool?)(this.canRead ??= GetPropertyInfo().CanRead);

        public MethodData GetMethodData
          => this.getMethodData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetPropertyInfo().GetGetMethod(true));

        public MethodData SetMethodData
          => this.setMethodData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetPropertyInfo().GetSetMethod(true));

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
          : this.symbolAttributes;

        public override SymbolComponentInfo SymbolComponentInfo
          => this.symbolComponentInfo ??= HelperExtensionsCommon.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

        public override string Signature
          => this.signature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string ShortSignature
          => this.shortSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

        public override string ShortCompactSignature
          => this.shortCompactSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

        public override string FullyQualifiedSignature
          => this.fullyQualifiedSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string FullyQualifiedRuntimeSignature
          => this.fullyQualifiedRuntimeSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeSignature
          => this.runtimeSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortSignature
          => this.runtimeShortSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortCompactSignature
          => this.runtimeShortCompactSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

        public override string DisplayName
          => this.displayName ??= HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string ShortDisplayName
          => this.shortDisplayName ??= HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

        public override string FullyQualifiedDisplayName
          => this.fullyQualifiedDisplayName ??= HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string AssemblyName
          => this.assemblyName ??= this.DeclaringTypeData.AssemblyName;

        public override bool IsStatic
          => (bool)(bool?)(this.isStatic ??= this.CanRead ? this.GetMethodData.IsStatic : this.SetMethodData.IsStatic);

#if !NETSTANDARD2_0
        public bool IsSetMethodReadOnly
          => (bool)(bool?)(this.isSetMethodReadOnly ??= this.CanWrite && this.SetMethodData.AttributeData.Any(data => data.AttributeType == typeof(IsReadOnlyAttribute)));
#endif

        public bool IsOverride
          => (bool)(bool?)(this.isOverride ??= this.CanRead ? this.GetMethodData.IsOverride : this.SetMethodData.IsOverride);
    }
}
