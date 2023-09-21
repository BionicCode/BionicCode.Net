namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;

  /// <summary>
  /// Use to decorate a member like method, constructor or property that should be profiled.
  /// </summary>
  /// <remarks>
  /// To start profiling create a configurator by calling <see cref="Profiler.CreateProfilerBuilder{T}"/>. This returns a <see cref="ProfilerBuilder"/> for a particular <see cref="Type"/> that allows the configuration of the profiling session.
  /// <br/>Finalize the step by calling <see cref="ProfilerBuilder.RunAsync"/> to start the profiling for the current type.
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
  public sealed class ProfileAttribute : Attribute
  {
    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="lineNumber"></param>
    public ProfileAttribute([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int lineNumber = -1)
    {
      this.SourceFilePath = sourceFilePath;
      this.LineNumber = lineNumber;
    }

    public string SourceFilePath { get; }
    public int LineNumber { get; }
  }
}