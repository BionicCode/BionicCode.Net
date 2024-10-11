
#if NET472_OR_GREATER

//[assembly: System.Reflection.AssemblyVersion("2.0")]
#endif
namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Management;
  using System.Runtime.InteropServices;
  using System.Threading;
  using System.Threading.Tasks;

  /// <summary>
  /// A collection of reusable static helper methods related to the development environment.
  /// </summary>
  public static class Environment
  {
    /// <summary>
    /// Checks whether the application is running in debug mode.
    /// </summary>
    /// <value><see langword="true"/> if the application is currently in debug mode. Otherwise <see langword="false"/>,</value>
    public static bool IsDebugModeEnabled =>
#if DEBUG
      true;
#else
      false;
#endif

    private static TaskCompletionSource<EnvironmentInfo> TaskCompletionSource { get; set; }

    private static EnvironmentInfo Info { get; set; }

    private static Dictionary<ManagementOperationObserver, ManagementObjectSearcher> ManagementObjectSearcherTable { get; } = new Dictionary<ManagementOperationObserver, ManagementObjectSearcher>();
    private static SemaphoreSlim Semaphore { get; set; } = new SemaphoreSlim(1, 1);

    public static async ValueTask<EnvironmentInfo> GetEnvironmentInfoAsync()
    {
      if (Environment.Info != default)
      {
        return Environment.Info;
      }

      await Environment.Semaphore?.WaitAsync();
      ManagementObjectSearcher searcher = null;
      try
      {
        if (Environment.Info != default)
        {
          return Environment.Info;
        }

        Environment.TaskCompletionSource = new TaskCompletionSource<EnvironmentInfo>();

        searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
        
        try
        {
          var watcher = new ManagementOperationObserver();
          Environment.ManagementObjectSearcherTable.Add(watcher, searcher);
          watcher.ObjectReady += OnWmiProcessorQueryResultReady;
          watcher.Completed += OnWmiProcessorQueryCompleted;
          searcher.Get(watcher);
        }
        catch (ManagementException e)
        {
          _ = Environment.TaskCompletionSource.TrySetResult(EnvironmentInfo.Default);
        }

        return await Environment.TaskCompletionSource.Task;
      }
      finally
      {
        _ = Environment.Semaphore?.Release();
        Environment.Semaphore?.Dispose();
        Environment.Semaphore = null;
        searcher?.Dispose();
      }
    }

    private static void OnWmiProcessorQueryCompleted(object sender, CompletedEventArgs e)
    {
      var watcher = sender as ManagementOperationObserver;
      if (Environment.ManagementObjectSearcherTable.TryGetValue(watcher, out ManagementObjectSearcher searcher))
      {
        watcher.ObjectReady -= OnWmiProcessorQueryResultReady;
        watcher.Completed -= OnWmiProcessorQueryCompleted;
        searcher.Dispose();
      }
    }

    private static void OnWmiProcessorQueryResultReady(object sender, ObjectReadyEventArgs e)
    {
      int clckSpeed = Convert.ToInt32(e.NewObject["CurrentClockSpeed"]);
      string processorName = e.NewObject["Name"] as string;
      int numberOfCores = Convert.ToInt32(e.NewObject["NumberOfCores"]);
      int numberOfLogicalCores = Convert.ToInt32(e.NewObject["NumberOfLogicalProcessors"]);
      int threadCount = Convert.ToInt32(e.NewObject["ThreadCount"]);
      Environment.Info = new EnvironmentInfo(processorName, 
        numberOfCores, 
        numberOfLogicalCores, 
        threadCount, 
        clckSpeed,
        System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower(),
        System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString().ToLower(),
        System.Runtime.InteropServices.RuntimeInformation.OSDescription,
        System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
        System.Environment.MachineName);

      _ = Environment.TaskCompletionSource.TrySetResult(Environment.Info);
    }
  }
}
