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

    public abstract HashSet<CustomAttributeData> AttributeData { get; }
    public abstract SymbolAttributes SymbolAttributes { get; }
    public string Name { get; }
    public abstract char[] FullyQualifiedSignature { get; }
    public abstract char[] Signature { get; }
  }
}