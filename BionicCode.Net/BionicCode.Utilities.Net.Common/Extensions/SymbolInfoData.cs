namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Reflection;

  [DebuggerDisplay("{RuntimeShortSignature}")]
  internal abstract class SymbolInfoData
  {
    protected SymbolInfoData(string name)
    {
      this.Name = name;
    }

    internal const BindingFlags AllMembersFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
    public abstract IList<CustomAttributeData> AttributeData { get; }
    public abstract SymbolAttributes SymbolAttributes { get; }
    public string Name { get; }
    public abstract string AssemblyName { get; }

    /// <summary>
    /// EventName without namespace, but with the declaring type (in case of a member), and generic type parameters.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// EventName without namespace and the declaring type (in case of a member), but <see langword="with"/>generic type parameters.
    /// </summary>
    public abstract string ShortDisplayName { get; }

    /// <summary>
    /// EventName with namespace, the declaring type (in case of a member), and generic type parameters.
    /// </summary>
    public abstract string FullyQualifiedDisplayName { get; }

    /// <summary>
    /// Signature including the namespace, the declaring type (in case of a member), attributes, generic type parameters and generic type parameter constraints.
    /// </summary>
    public abstract string FullyQualifiedSignature { get; }

    /// <summary>
    /// Signature without namespace, but with the declaring type (in case of a member), attributes, generic type parameters and generic type parameter constraints.
    /// </summary>
    public abstract string Signature { get; }

    /// <summary>
    /// Signature without namespace and the declaring type (in case of a member), but with attributes, generic type parameters and generic type parameter constraints.
    /// </summary>
    public abstract string ShortSignature { get; }

    /// <summary>
    /// Signature without namespace and the declaring type (in case of a member), attributes and generic type parameter constraints, but with generic type parameters.
    /// </summary>
    public abstract string ShortCompactSignature { get; }

    /// <summary>
    /// Signature without namespace and the declaring type (in case of a member), attributes and generic type parameter constraints, but with generic type parameters.
    /// <br/>This returns the constructed runtime signature for generic symbols where generic type parameters are replaced by their actual constructed runtime types (<c>Action&lt;T&gt;</c> becomes <c>Action&lt;int&gt;</c>)
    /// </summary>
    public abstract string RuntimeShortSignature { get; }

    ///// <summary>
    ///// Signature including the namespace and the declaring type (in case of a member), but without attributes and generic type parameter constraints, but with generic type parameters.
    ///// <br/>This returns the constructed runtime signature for generic symbols where generic type parameters are replaced by their actual constructed runtime types (<c>Action&lt;T&gt;</c> becomes <c>Action&lt;int&gt;</c>)
    ///// </summary>
    //public abstract string RuntimeSignature { get; }


    /// <summary>
    /// The individual components that make the signature.
    /// </summary>
    public abstract SymbolComponentInfo SymbolComponentInfo { get; }
  }
}