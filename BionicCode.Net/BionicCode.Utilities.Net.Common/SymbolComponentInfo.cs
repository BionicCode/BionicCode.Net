namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using System.Collections.ObjectModel;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Primitives;

  internal class SymbolComponentInfo
  {
    public ReadOnlyCollection<string> Modifiers { get; }
    public ReadOnlyCollection<SymbolComponentInfo> GenericTypeParameters { get; }
    public ReadOnlyCollection<SymbolComponentInfo> InheritedTypes { get; }
    public ReadOnlyCollection<SymbolComponentInfo> GenericTypeConstraints { get; }
    public ReadOnlyCollection<SymbolComponentInfo> CustomAttributes { get; }
    public ReadOnlyCollection<string> CustomAttributeConstructorArgs { get; }
    public ReadOnlyCollection<(string PropertyName, string PropertyValue)> CustomAttributeNamedArgs { get; }
    public ReadOnlyCollection<SymbolComponentInfo> Parameters { get; }
    public StringBuilder NameBuilder { get; }
    public StringBuilder ValueNameBuilder { get; }
    public bool IsKeyword { get; set; }
    public bool IsExtensionMethodParameter { get; set; }
    public bool IsSymbol { get; set; }
    public SymbolComponentInfo ReturnType
    {
      get => this.returnType;
      set
      {
        this.returnType = value;
        if (this.returnType != null)
        {
          this.returnType.IsSymbol = false;
        }
      }
    }
    public SymbolComponentInfo PropertyGet { get; set; }
    public SymbolComponentInfo PropertySet { get; set; }
    public string Signature { get; set; }
    public bool HasExpressionTerminator { get; set; }
    public bool IsIndexer { get; set; }
    public bool IsInitProperty { get; set; }
    public bool IsParameter { get; set; }
    public bool HasInlineAttributes { get; set; }

    private readonly List<string> modifiersInternal;
    private readonly List<SymbolComponentInfo> genericTypeParametersInternal;
    private readonly List<SymbolComponentInfo> inheritedTypesInternal;
    private readonly List<SymbolComponentInfo> genericTypeConstraintsInternal;
    private readonly List<SymbolComponentInfo> customAttributes;
    private readonly List<string> customAttributeConstructorArgs;
    private readonly List<(string PropertyName, string PropertyValue)> customAttributeNamedArgs;
    private readonly List<SymbolComponentInfo> parametersInternal;
    private string html;
    private SymbolComponentInfo returnType;

    public SymbolComponentInfo(bool isKeyword = false)
    {
      this.modifiersInternal = new List<string>();
      this.Modifiers = new ReadOnlyCollection<string>(this.modifiersInternal);
      this.genericTypeParametersInternal = new List<SymbolComponentInfo>();
      this.GenericTypeParameters = new ReadOnlyCollection<SymbolComponentInfo>(this.genericTypeParametersInternal);
      this.inheritedTypesInternal = new List<SymbolComponentInfo>();
      this.InheritedTypes = new ReadOnlyCollection<SymbolComponentInfo>(this.inheritedTypesInternal);
      this.genericTypeConstraintsInternal = new List<SymbolComponentInfo>();
      this.GenericTypeConstraints = new ReadOnlyCollection<SymbolComponentInfo>(this.genericTypeConstraintsInternal);
      this.parametersInternal = new List<SymbolComponentInfo>();
      this.Parameters = new ReadOnlyCollection<SymbolComponentInfo>(this.parametersInternal);
      this.customAttributes = new List<SymbolComponentInfo>();
      this.CustomAttributes = new ReadOnlyCollection<SymbolComponentInfo>(this.customAttributes);
      this.customAttributeConstructorArgs = new List<string>();
      this.CustomAttributeConstructorArgs = new ReadOnlyCollection<string>(this.customAttributeConstructorArgs);
      this.customAttributeNamedArgs = new List<(string PropertyName, string PropertyValue)>();
      this.CustomAttributeNamedArgs = new ReadOnlyCollection<(string PropertyName, string PropertyValue)>(this.customAttributeNamedArgs);
      this.NameBuilder = StringBuilderFactory.GetOrCreate();
      this.ValueNameBuilder = StringBuilderFactory.GetOrCreate();
      this.Signature = string.Empty;
      this.ReturnType = null;
      this.IsKeyword = isKeyword;
    }

    public SymbolComponentInfo(string name, bool isKeyword = false) : this(isKeyword)
    {
      _ = this.NameBuilder.Append(name);
    }

    public void AddModifier(string modifier)
      => this.modifiersInternal.Add(modifier);

    public void AddCustomAttribute(SymbolComponentInfo attribute)
      => this.customAttributes.Add(attribute);

    public void AddCustomAttributeConstructorArg(string attributeConstructorArg)
      => this.customAttributeConstructorArgs.Add(attributeConstructorArg);

    public void AddCustomAttributeNamedArg((string PropertyName, string PropertyValue) attributeNamedArg)
      => this.customAttributeNamedArgs.Add(attributeNamedArg);

    public void AddGenericTypeParameter(SymbolComponentInfo typeParameter)
      => this.genericTypeParametersInternal.Add(typeParameter);

    public void AddGenericTypeParameterRange(IEnumerable<SymbolComponentInfo> typeParameters) 
      => this.genericTypeParametersInternal.AddRange(typeParameters);

    public void AddGenericTypeConstraint(SymbolComponentInfo typeConstraint)
      => this.genericTypeConstraintsInternal.Add(typeConstraint);

    public void AddGenericTypeConstraintRange(IEnumerable<SymbolComponentInfo> typeConstraints)
      => this.genericTypeConstraintsInternal.AddRange(typeConstraints);

    public void AddInheritedType(SymbolComponentInfo type)
      => this.inheritedTypesInternal.Add(type);

    public void AddInheritedTypeRange(IEnumerable<SymbolComponentInfo> types)
      => this.inheritedTypesInternal.AddRange(types);

    public void AddParameter(SymbolComponentInfo parameter)
      => this.parametersInternal.Add(parameter);

    public override string ToString() => this.Signature;

    public string ToHtml()
    {
      if (this.html is null)
      {
        StringBuilder signatureBuilder = StringBuilderFactory.GetOrCreate()
          .Append("<div style=\"display: block; width: 100%;\">")
          .ToInlineHtml(this)
          .Append("</div>");

        this.html = signatureBuilder.ToString();
        StringBuilderFactory.Recycle(signatureBuilder);
      }

      return this.html;
    }
  }
}
