namespace BionicCode.Utilities.Net.Examples.ProfilerExamples.CreateProfilerBuilder.D
{
  using BionicCode.Utilities.Net.Examples.ProfilerExamples.B;
  #region CodeWithoutNamespace
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;

  class CreateProfilerBuilderExample
  {
    public static async Task Main(string[] args)
    {
      // Define a list of Types objects that should be profiled.
      // The members of these types must be decorated with the required attributes.
      var typesToProfile = new List<Type> { typeof(BenchmarkTarget) };
      ProfiledTypeResultCollection results = await Profiler.CreateProfilerBuilder(typesToProfile)
        .SetBaseUnit(TimeUnit.Milliseconds)
        .RunAsync(CancellationToken.None);
    }
  }

  #endregion
}
