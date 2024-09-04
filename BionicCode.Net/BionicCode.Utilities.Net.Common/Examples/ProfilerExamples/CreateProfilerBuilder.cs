namespace BionicCode.Utilities.Net.Examples.ProfilerExamples.CreateProfilerBuilder.A
{
  using BionicCode.Utilities.Net.Examples.ProfilerExamples.B;
  using System;
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

      foreach (ProfilerBatchResultGroupCollection resultGroups in results)
      {
        Console.WriteLine($"Profiled type: {resultGroups.ProfiledType.FullName}");
        foreach (ProfilerBatchResultGroup resultGroup in resultGroups)
        {
          Console.WriteLine($"Profiled member: {resultGroup.ProfiledTargetMemberShortName}");
          foreach (ProfilerBatchResult result in resultGroup)
          {
            Console.WriteLine($"Summary: {result.Summary}");
          }
        }
      }
    }
  }
  #endregion
}
