namespace BionicCode.Utilities.Net.Examples.ProfilerExamples.CreateProfilerBuilder.C
{
  #region CodeWithoutNamespace
  using BionicCode.Utilities.Net.Examples.ProfilerExamples.B;
  using System;
  using System.Threading.Tasks;

  class CreateProfilerBuilderExample
  {
    public static async Task Main(string[] args)
    {
      // Define a list of Type objects that should be profiled.
      // The members of these types must be decorated with the required attributes.
      Type[] typesToProfile = new[] { typeof(BenchmarkTarget) };
      ProfiledTypeResultCollection results = await Profiler.CreateProfilerBuilder(typesToProfile)
        .SetBaseUnit(TimeUnit.Milliseconds)
        .RunAsync();
    }
  }

#endregion
}
