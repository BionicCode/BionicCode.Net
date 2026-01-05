
#if NET472_OR_GREATER

//[assembly: System.Reflection.AssemblyVersion("2.0")]
#endif
namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public readonly struct EnvironmentInfo : IEquatable<EnvironmentInfo>
    {
        private const double NanosecondsPerSecond = 1E9;
        public static EnvironmentInfo Default { get; } = new EnvironmentInfo("Unknown", -1, -1, -1, -1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

        public EnvironmentInfo(string processorName, int processorCoreCount, int processorLogicalCoreCount, int threadCount, int processorSpeed, string processArchitecture, string operatingSystemArchitecture, string operatingSystemName, string runtimeVersion, string machineName)
        {
            this.ProcessorName = processorName;
            this.ProcessorCoreCount = processorCoreCount;
            this.ProcessorLogicalCoreCount = processorLogicalCoreCount;
            this.ThreadCount = threadCount;
            this.ProcessorSpeed = processorSpeed;
            this.ProcessArchitecture = processArchitecture;
            this.OperatingSystemArchitecture = operatingSystemArchitecture;
            this.RuntimeVersion = runtimeVersion;
            this.NanosecondsPerTick = NanosecondsPerSecond / Stopwatch.Frequency;
            this.OperatingSystemName = operatingSystemName;
            this.MachineName = machineName;
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

        public bool Equals(EnvironmentInfo other) => this.RuntimeVersion.Equals(other.RuntimeVersion, StringComparison.Ordinal)
          && this.OperatingSystemArchitecture.Equals(other.OperatingSystemArchitecture, StringComparison.Ordinal)
          && this.ProcessArchitecture.Equals(other.ProcessArchitecture, StringComparison.Ordinal)
          && this.ThreadCount == other.ThreadCount
          && this.ProcessorLogicalCoreCount == other.ProcessorLogicalCoreCount
          && this.ProcessorCoreCount == other.ProcessorCoreCount
          && this.ProcessorSpeed == other.ProcessorSpeed
          && this.ProcessorName.Equals(other.ProcessorName, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is EnvironmentInfo environmentInfo && Equals(environmentInfo);

        public override int GetHashCode()
        {
            int hashCode = -706515764;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(this.ProcessorName);
            hashCode = (hashCode * -1521134295) + this.ProcessorCoreCount.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.ProcessorSpeed.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.ProcessorLogicalCoreCount.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.ThreadCount.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.ProcessArchitecture.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.OperatingSystemArchitecture.GetHashCode();
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(this.RuntimeVersion);
            return hashCode;
        }

        public static bool operator ==(EnvironmentInfo left, EnvironmentInfo right) => left.Equals(right);
        public static bool operator !=(EnvironmentInfo left, EnvironmentInfo right) => !(left == right);
    }
}
