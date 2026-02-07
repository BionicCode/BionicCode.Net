namespace BionicCode.Utilities.Net.Profiling
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    internal class RuntimeContextProfilerConfiguration : IAttributeProfilerConfiguration
    {
        public RuntimeContextProfilerConfiguration(Runtime runtime, IEnumerable<TypeData> typeData, IAttributeProfilerConfiguration profilerConfigurationToCopy)
        {
            Runtime = runtime;
            TypeData = new HashSet<TypeData>(typeData);
            IsWarmupEnabled = profilerConfigurationToCopy.IsWarmupEnabled;
            IsDefaultLogOutputEnabled = profilerConfigurationToCopy.IsDefaultLogOutputEnabled;
            Iterations = profilerConfigurationToCopy.Iterations;
            WarmupIterations = profilerConfigurationToCopy.WarmupIterations;
            BaseUnit = profilerConfigurationToCopy.BaseUnit;
            AsyncProfilerLogger = profilerConfigurationToCopy.AsyncProfilerLogger;
            ProfilerLogger = profilerConfigurationToCopy.ProfilerLogger;
            AutoDiscoverSourceAssemblies = Array.Empty<Assembly>();
            IsAutoDiscoverEnabled = false;
        }

        public Runtime Runtime { get; }
        public HashSet<TypeData> TypeData { get; }
        public bool IsWarmupEnabled { get; }
        public bool IsDefaultLogOutputEnabled { get; }
        public int Iterations { get; }
        public int WarmupIterations { get; }
        public TimeUnit BaseUnit { get; }
        public Func<ProfilerBatchResult, string, Task> AsyncProfilerLogger { get; }
        public Action<ProfilerBatchResult, string> ProfilerLogger { get; }
        public bool IsAutoDiscoverEnabled { get; }
        public Assembly[] AutoDiscoverSourceAssemblies { get; }

        public Assembly GetAssembly(Type type) => throw new NotImplementedException();
    }
}
