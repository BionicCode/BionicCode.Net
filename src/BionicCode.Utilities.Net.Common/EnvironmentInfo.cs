
#if NET472_OR_GREATER

//[assembly: System.Reflection.AssemblyVersion("2.0")]
#endif
namespace BionicCode.Utilities.Net;

using System;
using System.Collections.Generic;
using System.Diagnostics;

public readonly struct EnvironmentInfo : IEquatable<EnvironmentInfo>
{
    private const double NanosecondsPerSecond = 1E9;
    public static EnvironmentInfo Default { get; } = new EnvironmentInfo("Unknown", -1, -1, -1, -1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    public EnvironmentInfo(string processorName, int processorCoreCount, int processorLogicalCoreCount, int threadCount, int processorSpeed, string processArchitecture, string operatingSystemArchitecture, string operatingSystemName, string runtimeVersion, string machineName)
    {
        ProcessorName = processorName;
        ProcessorCoreCount = processorCoreCount;
        ProcessorLogicalCoreCount = processorLogicalCoreCount;
        ThreadCount = threadCount;
        ProcessorSpeed = processorSpeed;
        ProcessArchitecture = processArchitecture;
        OperatingSystemArchitecture = operatingSystemArchitecture;
        RuntimeVersion = runtimeVersion;
        NanosecondsPerTick = NanosecondsPerSecond / Stopwatch.Frequency;
        OperatingSystemName = operatingSystemName;
        MachineName = machineName;
    }

    public bool HasHighPrecisionTimer => Stopwatch.IsHighResolution;
    public double NanosecondsPerTick { get; }
    public string ProcessorName { get; }
    public int ProcessorCoreCount { get; }
    public int ProcessorSpeed { get; }
    public int ProcessorLogicalCoreCount { get; }
    public int ThreadCount { get; }
    public string ProcessArchitecture { get; }
    public string OperatingSystemArchitecture { get; }
    public string OperatingSystemName { get; }
    public string RuntimeVersion { get; }
    public string MachineName { get; }

    public bool Equals(EnvironmentInfo other) => RuntimeVersion.Equals(other.RuntimeVersion, StringComparison.Ordinal)
      && OperatingSystemArchitecture.Equals(other.OperatingSystemArchitecture, StringComparison.Ordinal)
      && ProcessArchitecture.Equals(other.ProcessArchitecture, StringComparison.Ordinal)
      && ThreadCount == other.ThreadCount
      && ProcessorLogicalCoreCount == other.ProcessorLogicalCoreCount
      && ProcessorCoreCount == other.ProcessorCoreCount
      && ProcessorSpeed == other.ProcessorSpeed
      && ProcessorName.Equals(other.ProcessorName, StringComparison.Ordinal);

    public override bool Equals(object obj) => obj is EnvironmentInfo environmentInfo && Equals(environmentInfo);

    public override int GetHashCode()
    {
        return HashCode.Combine(ProcessorName, ProcessorCoreCount, ProcessorSpeed, ProcessorLogicalCoreCount, ThreadCount, ProcessArchitecture, OperatingSystemArchitecture, RuntimeVersion);
    }

    public static bool operator ==(EnvironmentInfo left, EnvironmentInfo right) => left.Equals(right);
    public static bool operator !=(EnvironmentInfo left, EnvironmentInfo right) => !(left == right);
}
