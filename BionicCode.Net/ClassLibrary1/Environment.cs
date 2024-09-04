
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
    private static SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);

    public static async ValueTask<EnvironmentInfo> GetEnvironmentInfoAsync()
    {
      if (Environment.Info != default)
      {
        return Environment.Info;
      }

      await Environment.Semaphore.WaitAsync();
      try
      {
        if (Environment.Info != default)
        {
          return Environment.Info;
        }

        Environment.TaskCompletionSource = new TaskCompletionSource<EnvironmentInfo>();

        ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");

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
        _ = Environment.Semaphore.Release();
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
        System.Environment.Is64BitProcess, 
        System.Environment.Is64BitOperatingSystem, 
        RuntimeEnvironment.GetSystemVersion());
      _ = Environment.TaskCompletionSource.TrySetResult(Environment.Info);
    }
  }

  public readonly struct EnvironmentInfo : IEquatable<EnvironmentInfo>
  {
    private static double NanosecondsPerSecond = 1E9;
    public static EnvironmentInfo Default { get; } = new EnvironmentInfo("Unknown", -1, System.Environment.ProcessorCount, -1, -1, System.Environment.Is64BitProcess, System.Environment.Is64BitOperatingSystem, RuntimeEnvironment.GetSystemVersion());

    public EnvironmentInfo(string processorName, int processorCoreCount, int processorLogicalCoreCount, int thradCount, int processorSpeed, bool is64BitProcess, bool is64BitOperatingSystem, string runtimeVersion)
    {
      this.ProcessorName = processorName;
      this.ProcessorCoreCount = processorCoreCount;
      this.ProcessorLogicalCoreCount = processorLogicalCoreCount;
      this.ThradCount = thradCount;
      this.ProcessorSpeed = processorSpeed;
      this.Is64BitProcess = is64BitProcess;
      this.Is64BitOperatingSystem = is64BitOperatingSystem;
      this.RuntimeVersion = runtimeVersion;
      this.NanosecondsPerTick = NanosecondsPerSecond / Stopwatch.Frequency;
    }

    public bool HasHighPrecisionTimer => Stopwatch.IsHighResolution;
    public double NanosecondsPerTick { get; }
    public string ProcessorName { get; }
    public int ProcessorCoreCount { get; }
    public int ProcessorSpeed { get; }
    public int ProcessorLogicalCoreCount { get; }
    public int ThradCount { get; }
    public bool Is64BitProcess { get; }
    public bool Is64BitOperatingSystem { get; }
    public string RuntimeVersion { get; }

    public bool Equals(EnvironmentInfo other) =>  this.RuntimeVersion == other.RuntimeVersion
      && this.Is64BitOperatingSystem == other.Is64BitOperatingSystem
      && this.Is64BitProcess == other.Is64BitProcess
      && this.ThradCount == other.ThradCount
      && this.ProcessorLogicalCoreCount == other.ProcessorLogicalCoreCount
      && this.ProcessorCoreCount == other.ProcessorCoreCount
      && this.ProcessorSpeed == other.ProcessorSpeed
      && this.ProcessorName == other.ProcessorName;

    public override bool Equals(object obj) => base.Equals(obj);

    public override int GetHashCode()
    {
      int hashCode = -706515764;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.ProcessorName);
      hashCode = hashCode * -1521134295 + this.ProcessorCoreCount.GetHashCode();
      hashCode = hashCode * -1521134295 + this.ProcessorSpeed.GetHashCode();
      hashCode = hashCode * -1521134295 + this.ProcessorLogicalCoreCount.GetHashCode();
      hashCode = hashCode * -1521134295 + this.ThradCount.GetHashCode();
      hashCode = hashCode * -1521134295 + this.Is64BitProcess.GetHashCode();
      hashCode = hashCode * -1521134295 + this.Is64BitOperatingSystem.GetHashCode();
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.RuntimeVersion);
      return hashCode;
    }

    public static bool operator ==(EnvironmentInfo left, EnvironmentInfo right) => left.Equals(right);
    public static bool operator !=(EnvironmentInfo left, EnvironmentInfo right) => !(left == right);
  }
}
