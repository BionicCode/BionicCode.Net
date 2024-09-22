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
    public abstract HashSet<CustomAttributeData> AttributeData { get; }
    public abstract SymbolAttributes SymbolAttributes { get; }
    public string Name { get; }
    public abstract string DisplayName { get; }
    public abstract string FullyQualifiedDisplayName { get; }
    public abstract string FullyQualifiedSignature { get; }
    public abstract string Signature { get; }
  }
}