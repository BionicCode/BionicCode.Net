namespace BionicCode.Utilities.Net
{
  using System;
  using System.Reflection;
  using System.Runtime.Versioning;

  // TODO::Move context related prperties properties from ProfilerBtchResult to Context class and compose
  public class ProfilerContext
  {
    public ProfilerContext(string assemblyName, string targetName, ProfiledTargetType targetType, string sourceFileName, int lineNumber, MemberInfo targetTypeInfo, int warmupCount, Runtime runtime)
    {
      this.AssemblyName = assemblyName;
      this.SourceFileName = sourceFileName;
      this.LineNumber = lineNumber;
      this.TargetName = targetName;
      this.TargetType = targetType;
      this.RuntimeVersionFactory = new Lazy<string>(() => Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName);
      this.TargetTypeInfo = targetTypeInfo;
      this.WarmupCount = warmupCount;
      this.Runtime = runtime;
    }

    public Runtime Runtime { get; }
    public string AssemblyName { get; }
    public MemberInfo TargetTypeInfo { get; }
    public string SourceFileName { get; }
    public int LineNumber { get; }
    public string TargetName { get; }
    public ProfiledTargetType TargetType { get; }
    public Lazy<string> RuntimeVersionFactory { get; }
    public string RuntimeVersion => this.RuntimeVersionFactory.Value;
    public int WarmupCount { get; }
  }
}