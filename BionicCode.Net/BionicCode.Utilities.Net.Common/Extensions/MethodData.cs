namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using Microsoft.CodeAnalysis;

  internal sealed class MethodData : MemberInfoData
  {
    private SymbolAttributes symbolAttributes;
    private AccessModifier accessModifier;
    private bool? isAwaitable;
    private bool? isAsync;
    private bool? isSealed;
    private bool? isExtensionMethod;
    private ParameterData[] parameters;
    private TypeData[] genericTypeArguments;
    private bool? isOverride;
    private bool? isStatic;
    private string signature;
    private string fullyQualifiedSignature;
    private TypeData returnTypeData;
    private bool? isGenericMethod;
    private bool? isGenericTypeMethod;
    private MethodData genericMethodDefinitionData;
    private bool? isReturnValueByRef;

#if !NETSTANDARD2_0
    private bool? isReturnValueReadOnly;
#endif

    public MethodData(MethodInfo methodInfo) : base(methodInfo)
    {
      this.Handle = methodInfo.MethodHandle;
    }

    public MethodInfo GetMethodInfo()
      => (MethodInfo)MethodInfo.GetMethodFromHandle(this.Handle, this.DeclaringTypeHandle);

    protected override MemberInfo GetMemberInfo() 
      => GetMethodInfo();

    public RuntimeMethodHandle Handle { get; }

    public MethodData GenericMethodDefinitionData
    {
      get
      {
        if (this.IsGenericMethodDefinition)
        {
          return this;
        }
        else
        {
          MethodInfo genericMethodDefinitionMethodInfo = GetMethodInfo().GetGenericMethodDefinition();

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
          this.genericMethodDefinitionData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<MethodData>(genericMethodDefinitionMethodInfo);
        }
After:
          this.genericMethodDefinitionData = Net.SymbolReflectionInfoCache.GetSymbolInfoDataCacheEntry<MethodData>(genericMethodDefinitionMethodInfo);
        }
*/
          this.genericMethodDefinitionData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry<MethodData>(genericMethodDefinitionMethodInfo);
        }

        return this.genericMethodDefinitionData;
      }
    }

    public ParameterData[] Parameters 

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
      => this.parameters ?? (this.parameters = GetMethodInfo().GetParameters().Select(HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<ParameterData>).ToArray());
After:
      => this.parameters ?? (this.parameters = GetMethodInfo().GetParameters().Select(Net.SymbolReflectionInfoCache.GetSymbolInfoDataCacheEntry<ParameterData>).ToArray());
*/
      => this.parameters ?? (this.parameters = GetMethodInfo().GetParameters().Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry<ParameterData>).ToArray());

    public TypeData[] GenericTypeArguments 
    {
      get
      {
        if (this.genericTypeArguments is null)
        {
          Type[] typeArguments = GetMethodInfo().GetGenericArguments();

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
          this.genericTypeArguments = typeArguments.Select(HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>).ToArray();
        }
After:
          this.genericTypeArguments = typeArguments.Select(Net.SymbolReflectionInfoCache.GetSymbolInfoDataCacheEntry<TypeData>).ToArray();
        }
*/
          this.genericTypeArguments = typeArguments.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry<TypeData>).ToArray();
        }

        return this.genericTypeArguments;
      }
    }

    public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
      : this.accessModifier;

    public bool IsExtensionMethod 
      => (bool)(this.isExtensionMethod ?? (this.isExtensionMethod = HelperExtensionsCommon.IsExtensionMethodInternal(this)));

    public bool IsAsync 
      => (bool)(this.isAsync ?? (this.isAsync = HelperExtensionsCommon.IsMarkedAsyncInternal(this)));

    public bool IsAwaitable 
      => (bool)(this.isAwaitable ?? (this.isAwaitable = HelperExtensionsCommon.IsAwaitableInternal(this)));

    public bool IsOverride 
      => (bool)(this.isOverride ?? (this.isOverride = HelperExtensionsCommon.IsOverrideInternal(this)));

    public override bool IsStatic 
      => (bool)(this.isStatic ?? (this.isStatic = GetMethodInfo().IsStatic));

    public bool IsSealed
      => (bool)(this.isSealed ?? (this.isSealed = GetMethodInfo().IsFinal));

#if !NETSTANDARD2_0
    public bool IsReturnValueReadOnly
      => (bool)(this.isReturnValueReadOnly ?? (this.isReturnValueReadOnly = GetMethodInfo().ReturnParameter.GetCustomAttribute(typeof(IsReadOnlyAttribute)) != null));
#endif

    public bool IsReturnValueByRef
      => (bool)(this.isReturnValueByRef ?? (this.isReturnValueByRef = this.ReturnTypeData.IsByRef));

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override string Signature
      => this.signature ?? (this.signature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: false));

    public override string FullyQualifiedSignature 
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isShortName: true, isCompact: false));

    public TypeData ReturnTypeData 

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
      => this.returnTypeData ?? (this.returnTypeData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>(GetMethodInfo().ReturnType));
After:
      => this.returnTypeData ?? (this.returnTypeData = Net.SymbolReflectionInfoCache.GetSymbolInfoDataCacheEntry<TypeData>(GetMethodInfo().ReturnType));
*/
      => this.returnTypeData ?? (this.returnTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry<TypeData>(GetMethodInfo().ReturnType));

    public bool IsGenericMethod
      => (bool)(this.isGenericMethod ?? (this.isGenericMethod = GetMethodInfo().IsGenericMethod));

    public bool IsGenericMethodDefinition
      => (bool)(this.isGenericTypeMethod ?? (this.isGenericTypeMethod = GetMethodInfo().IsGenericMethodDefinition));
  }
}