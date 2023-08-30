namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// Use to decorate a member like method, constructor or property that should be profiled with particular argument list.
  /// </summary>
  /// <remarks>
  /// To start profiling create a configurator by calling <see cref="Profiler.LogType{T}"/>. This returns a <see cref="ProfilerBuilder"/> for a particular <see cref="Type"/> that allows the configuration of the profiling session.
  /// <br/>Finalize the step by calling <see cref="ProfilerBuilder.RunAsync"/> to start the profiling for the current type.
  /// <para>
  /// A member can be decorated with multiple <see cref="ProfilerArgumentAttribute"/> attributes. This is useful to emulate real-world usage where the input is dynamic and may impact the perfomance of the profiled member. 
  /// <br/>Each <see cref="ProfilerArgumentAttribute"/> results in an profiling iteration. If the configured iterations of the <see cref="ProfilerBuilder"/> exceed the number of <see cref="ProfilerArgumentAttribute"/> attributes, then the profiler will alterante between the argument lists.
  /// <br/>The default number of iterations is <c>1</c> and can be customized by calling <see cref="ProfilerBuilder.SetIterations(int)"/>.
  /// </para>
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
  public sealed class ProfilerArgumentAttribute : Attribute
  {
    /// <summary>
    /// Constructor to request the ordered argument list for the decorated member.
    /// </summary>
    /// <param name="arguments">The argument list ordered by their position.</param>
    public ProfilerArgumentAttribute(params object[] arguments) => this.Arguments = arguments;

    /// <summary>
    /// Gets the argument list. It is assumed that the provided list is ordered by position.
    /// </summary>
    /// <value>
    /// The argument list that is used to profile the decorated member.
    /// </value>
    public IEnumerable<object> Arguments { get; }

    /// <summary>
    /// The index used to get/set an indexer.
    /// </summary>
    public object Index { get; set; }
  }
}