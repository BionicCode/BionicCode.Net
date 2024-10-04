namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  using System.Reflection;

  internal abstract class ProfiledMemberInfo
  {
    public abstract MemberInfoData MemberInfoData { get; }
    public string AssemblyName { get; }
    public int LineNumber { get; }
    public string SourceFilePath { get; }
    public Runtime TargetFramework { get; }
    public IList<IEnumerable<object>> ArgumentLists { get; }
    public bool IsStatic { get; }
    public string Name => this.MemberInfoData.Name;
    public string DisplayName => this.MemberInfoData.DisplayName;
    public string ShortDisplayName => this.MemberInfoData.ShortDisplayName;
    public string Namespace => this.MemberInfoData.Namespace;
    public string Signature => this.MemberInfoData.Signature;
    public string ShortSignature => this.MemberInfoData.ShortSignature;
    public string ShortCompactSignature => this.MemberInfoData.ShortCompactSignature;

    protected ProfiledMemberInfo(IList<IEnumerable<object>> argumentLists, bool isStatic, string assemblyName, int lineNumber, string sourceFilePath, Runtime targetFramework)
    {
      this.ArgumentLists = argumentLists;
      this.IsStatic = isStatic;
      this.AssemblyName = assemblyName;
      this.LineNumber = lineNumber;
      this.SourceFilePath = sourceFilePath;
      this.TargetFramework = targetFramework;
    }
  }
}