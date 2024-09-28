namespace BionicCode.Utilities.Net.Examples.ProfilerExamples.CreateProfilerBuilder.B
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
      ProfiledTypeResultCollection results = await Profiler.CreateProfilerBuilder<BenchmarkTarget>()
        .SetBaseUnit(TimeUnit.Milliseconds)
        .RunAsync(CancellationToken.None);

      foreach (ProfilerBatchResultGroupCollection resultGroups in results)
      {
        Console.WriteLine($"Profiled type: {resultGroups.ProfiledTypeData.FullyQualifiedDisplayName}");
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
