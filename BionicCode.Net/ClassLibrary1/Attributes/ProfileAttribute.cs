namespace BionicCode.Utilities.Net
{
  using System;
  using System.Runtime.CompilerServices;

  /// <summary>
  /// Use to decorate a member like method, constructor or property that should be profiled.  /// 
  /// </summary>
  /// <remarks>
  /// To start profiling create a configurator by calling <see cref="Profiler.CreateProfilerBuilder{T}"/>. This returns a <see cref="ProfilerBuilder"/> for a particular <see cref="Type"/> that allows the configuration of the profiling session.
  /// <br/>Finalize the step by calling <see cref="ProfilerBuilder.RunAsync"/> to start the profiling for the current type.
  /// <para>
  /// By default the profiler uses the latest released .NET runtime to execute the profiling. Set the <seealso cref="ProfileAttribute.TargetFramework"/> property to define
  /// </para>
  /// </remarks>
  [System.AttributeUsage(
    System.AttributeTargets.Method | AttributeTargets.Delegate | AttributeTargets.Constructor | AttributeTargets.Property,
    Inherited = false,
    AllowMultiple = true)]
  public sealed class ProfileAttribute : Attribute
  {
    /// <summary>
    /// Constructor. 
    /// This default constructor configures the profiler to use the latest released .NET runtime to execute the profiling. 
    /// Set the <seealso cref="ProfileAttribute.TargetFramework"/> property or use the overload to configure a different .NET runtme.
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="lineNumber"></param>
    public ProfileAttribute([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int lineNumber = -1)
    {
      this.SourceFilePath = sourceFilePath;
      this.LineNumber = lineNumber;
      this.TargetFramework = Runtime.Current;
    }

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="targetFramework">The target .NET runtime that the profiler must use to execute the profiled code.</param>
    /// <param name="sourceFilePath"></param>
    /// <param name="lineNumber"></param>
    public ProfileAttribute(Runtime targetFramework, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int lineNumber = -1)
    {
      this.SourceFilePath = sourceFilePath;
      this.LineNumber = lineNumber;
      this.TargetFramework = targetFramework;
    }

    public string SourceFilePath { get; }
    public int LineNumber { get; }
    public Runtime TargetFramework { get; }
  }
}