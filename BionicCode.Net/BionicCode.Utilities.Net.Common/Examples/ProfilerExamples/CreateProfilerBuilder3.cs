namespace BionicCode.Utilities.Net.Examples.ProfilerExamples.CreateProfilerBuilder.C
{
  #region CodeWithoutNamespace
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net.Examples.ProfilerExamples.B;

  class CreateProfilerBuilderExample
  {
    public static async Task Main(string[] args)
    {
      // Define a list of Types objects that should be profiled.
      // The members of these types must be decorated with the required symbolAttributes.
      Type[] typesToProfile = new[] { typeof(BenchmarkTarget) };
      ProfiledTypeResultCollection results = await Profiler.CreateProfilerBuilder(typesToProfile)
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
