namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  using System.Reflection;

  internal abstract class ProfiledMemberInfo
  {
    public string AssemblyName { get; }
    public int LineNumber { get; }
    public string SourceFilePath { get; }
    public Runtime TargetFramework { get; }
    public IEnumerable<IEnumerable<object>> ArgumentLists { get; }
    public bool IsStatic { get; }
    public abstract MemberInfo MemberInfo { get; }

    protected ProfiledMemberInfo(IEnumerable<IEnumerable<object>> argumentLists, bool isStatic, string assemblyName, int lineNumber, string sourceFilePath, Runtime targetFramework)
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