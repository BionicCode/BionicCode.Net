namespace BionicCode.Utilities.Net.Examples.ProfilerExamples.CreateProfilerBuilder.A
{
  using BionicCode.Utilities.Net.Examples.ProfilerExamples.B;
  using System.Threading;
  #region CodeWithoutNamespace
  using System.Threading.Tasks;

  class CreateProfilerBuilderExample
  {
    public static async Task Main(string[] args)
    {
      ProfiledTypeResultCollection results = await Profiler.CreateProfilerBuilder(typeof(BenchmarkTarget))
        .SetBaseUnit(TimeUnit.Milliseconds)
        .RunAsync(CancellationToken.None);
    }
  }
  #endregion
}
