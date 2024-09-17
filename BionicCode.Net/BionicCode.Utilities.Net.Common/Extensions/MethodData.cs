namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;
  using System.Reflection;
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
    private char[] signature;
    private char[] fullyQualifiedSignature;
    private TypeData returnTypeData;
    private bool? isGenericMethod;
    private bool? isGenericTypeMethod;
    private MethodData genericMethodDefinitionData;

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
          this.genericMethodDefinitionData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<MethodData>(genericMethodDefinitionMethodInfo);
        }

        return this.genericMethodDefinitionData;
      }
    }

    public ParameterData[] Parameters 
      => this.parameters ?? (this.parameters = GetMethodInfo().GetParameters().Select(HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<ParameterData>).ToArray());

    public TypeData[] GenericTypeArguments 
    {
      get
      {
        if (this.genericTypeArguments is null)
        {
          Type[] typeArguments = GetMethodInfo().GetGenericArguments();
          this.genericTypeArguments = typeArguments.Select(HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>).ToArray();
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

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override char[] Signature 
      => this.signature ?? (this.signature = GetType().ToSignatureShortName().ToCharArray());

    public override char[] FullyQualifiedSignature 
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = GetType().ToSignatureShortName(isFullyQualifiedName: true).ToCharArray());

    public TypeData ReturnTypeData 
      => this.returnTypeData ?? (this.returnTypeData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>(GetMethodInfo().ReturnType));

    public bool IsGenericMethod
      => (bool)(this.isGenericMethod ?? (this.isGenericMethod = GetMethodInfo().IsGenericMethod));

    public bool IsGenericMethodDefinition
      => (bool)(this.isGenericTypeMethod ?? (this.isGenericTypeMethod = GetMethodInfo().IsGenericMethodDefinition));
  }
}