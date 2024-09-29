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
    public ReadOnlyCollection<(SymbolComponentInfo Type, string Name, bool IsKeyword)> Parameters { get; }
    public StringBuilder NameBuilder { get; }
    public SymbolComponentInfo ReturnType { get; set; }
    public string Signature { get; set; }

    private readonly List<string> modifiersInternal;
    private readonly List<SymbolComponentInfo> genericTypeParametersInternal;
    private readonly List<SymbolComponentInfo> inheritedTypesInternal;
    private readonly List<SymbolComponentInfo> genericTypeConstraintsInternal;
    private readonly List<(SymbolComponentInfo Type, string Name, bool IsKeyword)> parametersInternal;

    public SymbolComponentInfo()
    {
      this.modifiersInternal = new List<string>();
      this.Modifiers = new ReadOnlyCollection<string>(this.modifiersInternal);
      this.genericTypeParametersInternal = new List<SymbolComponentInfo>();
      this.GenericTypeParameters = new ReadOnlyCollection<SymbolComponentInfo>(this.genericTypeParametersInternal);
      this.inheritedTypesInternal = new List<SymbolComponentInfo>();
      this.InheritedTypes = new ReadOnlyCollection<SymbolComponentInfo>(this.inheritedTypesInternal);
      this.genericTypeConstraintsInternal = new List<SymbolComponentInfo>();
      this.GenericTypeConstraints = new ReadOnlyCollection<SymbolComponentInfo>(this.genericTypeConstraintsInternal);
      this.parametersInternal = new List<(SymbolComponentInfo Type, string Name, bool IsKeyword)>();
      this.Parameters = new ReadOnlyCollection<(SymbolComponentInfo Type, string Name, bool IsKeyword)>(this.parametersInternal);
      this.NameBuilder = StringBuilderFactory.GetOrCreate();
      this.Signature = string.Empty;
      this.ReturnType = null;
    }

    public SymbolComponentInfo(string name) : this()
    {
      _ = this.NameBuilder.Append(name);
    }

    public void AddModifier(string modifier)
      => this.modifiersInternal.Add(modifier);

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

    public void AddParameter((SymbolComponentInfo Type, string Name, bool IsKeyword) parameter)
      => this.parametersInternal.Add(parameter);

    public override string ToString() => this.Signature;

    public string ToHtml()
    {
      StringBuilder signatureBuilder = StringBuilderFactory.GetOrCreate()
        .Append("<div>");

      if (this.Modifiers.Any())
      {
        _ = signatureBuilder.Append($"<span class=\"syntax-keyword\">");
        foreach (string modifier in this.Modifiers)
        {
          _ = signatureBuilder.Append(modifier)
              .Append(' ');
        }
        
        _ = signatureBuilder.Append($"</span>");
      }

      if (this.ReturnType != null)
      {
        _ = signatureBuilder.Append(this.ReturnType.ToHtml())
            .Append(' ');
      }

      if (this.NameBuilder.Length > 0)
      {
        _ = signatureBuilder.Append($"<span class=\"syntax-symbol\">")
          .AppendStringBuilder(this.NameBuilder)
          .Append("</span>");
        StringBuilderFactory.Recycle(this.NameBuilder);
      }

      if (this.GenericTypeParameters.Any())
      {
        _ = signatureBuilder.Append($"<span class=\"syntax-delimiter\">")
          .AppendReadOnlySpan('<'.ToHtmlEncodedReadOnlySpan())
          .Append("</span>")
          .Append($"<span class=\"syntax-symbol\">");

        foreach (SymbolComponentInfo typeParameter in this.GenericTypeParameters)
        {
          _ = signatureBuilder.Append(typeParameter.ToHtml())
            .Append(", ");
        }

        _ = signatureBuilder.Remove(signatureBuilder.Length - 2, 2)
          .Append("</span>")
          .Append($"<span class=\"syntax-delimiter\">")
          .AppendReadOnlySpan('>'.ToHtmlEncodedReadOnlySpan())
          .Append("</span>");
      }

      if (this.Parameters.Any())
      {
        _ = signatureBuilder.Append($"<span class=\"syntax-delimiter\">")
          .AppendReadOnlySpan('('.ToHtmlEncodedReadOnlySpan())
          .Append("</span>")
          .Append($"<span class=\"syntax-symbol\">");

        foreach ((SymbolComponentInfo Type, string Name, bool IsKeyword) parameter in this.Parameters)
        {
          _ = signatureBuilder.Append(parameter.Type.ToHtml())
            .Append(' ')
            .Append(parameter.Name)
            .Append(", ");
        }

        _ = signatureBuilder.Remove(signatureBuilder.Length - 2, 2)
          .Append("</span>")
          .Append($"<span class=\"syntax-delimiter\">")
          .AppendReadOnlySpan(')'.ToHtmlEncodedReadOnlySpan())
          .Append("</span>");
      }

      _ = signatureBuilder.Append("<div>");

      return signatureBuilder.ToString();
    }
  }
}
