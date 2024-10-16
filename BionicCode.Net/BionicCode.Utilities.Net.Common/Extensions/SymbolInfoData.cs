namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  using System.Reflection;

  internal abstract class SymbolInfoData
  {
    protected SymbolInfoData(string name)
    {
      this.Name = name;
    }

    internal const BindingFlags AllMembersFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    public abstract IList<CustomAttributeData> AttributeData { get; }
    public abstract SymbolAttributes SymbolAttributes { get; }
    public string Name { get; }
    public abstract string AssemblyName { get; }

    /// <summary>
    /// Name without namespace, but with the declaring type (in case of a member), and generic type parameters.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Name without namespace and the declaring type (in case of a member), but <see langword="with"/>generic type parameters.
    /// </summary>
    public abstract string ShortDisplayName { get; }

    /// <summary>
    /// Name with namespace, the declaring type (in case of a member), and generic type parameters.
    /// </summary>
    public abstract string FullyQualifiedDisplayName { get; }

    /// <summary>
    /// Signature with namespace, the declaring type (in case of a member), attributes, generic type parameters and generic type parameter constraints.
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

    public abstract SymbolComponentInfo SymbolComponentInfo { get; }
  }
}