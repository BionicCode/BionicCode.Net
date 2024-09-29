﻿namespace BionicCode.Utilities.Net
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
    public abstract string DisplayName { get; }
    public abstract string ShortDisplayName { get; }
    public abstract string FullyQualifiedDisplayName { get; }
    public abstract string FullyQualifiedSignature { get; }
    public abstract string Signature { get; }
    public abstract string ShortSignature { get; }
  }
}