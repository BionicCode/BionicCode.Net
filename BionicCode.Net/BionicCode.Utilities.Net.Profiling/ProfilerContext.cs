namespace BionicCode.Utilities.Net.Profiling
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    // TODO::Move context related properties from ProfilerBatchResult to Context class and compose
    internal abstract class ProfilerContext
    {
        protected ProfilerContext(TypeData targetInstanceTypeData, string sourceFileName, int lineNumber, int warmupCount, int iterationCount, Runtime runtime, TimeUnit baseUnit, Action<ProfilerBatchResult, string> logger, Func<ProfilerBatchResult, string, Task> asyncLogger)
        {
            this.TargetInstanceTypeData = targetInstanceTypeData;
            this.FullSourceFileName = sourceFileName;
            this.LineNumber = lineNumber;
            this.RuntimeVersionFactory = new Lazy<string>(() => System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
            this.WarmupCount = warmupCount;
            this.IterationCount = iterationCount;
            this.Runtime = runtime;
            this.BaseUnit = baseUnit;
            this.Logger = logger;
            this.AsyncLogger = asyncLogger;
        }

        public Runtime Runtime { get; }
        public TypeData TargetInstanceTypeData { get; }
        public string FullSourceFileName { get; }
        public string SourceFileName => Path.GetFileName(this.FullSourceFileName);
        public int LineNumber { get; }
        public Lazy<string> RuntimeVersionFactory { get; }
        public string RuntimeVersion => this.RuntimeVersionFactory.Value;
        public int WarmupCount { get; }
        public int IterationCount { get; }
        public TimeUnit BaseUnit { get; }
        public Action<ProfilerBatchResult, string> Logger { get; }
        public Func<ProfilerBatchResult, string, Task> AsyncLogger { get; }
    }

    internal abstract class ProfilerContext<TTarget> : ProfilerContext
    {
        protected ProfilerContext(
            TTarget targetInstance,
            string sourceFileName,
            int lineNumber,
            int warmupCount,
            int iterationCount,
            Runtime runtime,
            TimeUnit baseUnit,
            Action<ProfilerBatchResult, string> logger,
            Func<ProfilerBatchResult, string, Task> asyncLogger) : base(typeof(TTarget).ToTypeData(), sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger)
        {
            this.TargetInstance = targetInstance;
        }

        public TTarget TargetInstance { get; }
    }

    internal class MethodProfilerContext<TTarget> : ProfilerContext<TTarget>
    {
        public MethodProfilerContext(
            TTarget targetInstance,
            MethodData methodData,
            string sourceFileName,
            int lineNumber,
            int warmupCount,
            int iterationCount,
            Runtime runtime,
            TimeUnit baseUnit,
            Action<ProfilerBatchResult, string> logger,
            Func<ProfilerBatchResult, string, Task> asyncLogger) : base(targetInstance, sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger)
        {
            this.MethodData = methodData;
        }

        public MethodData MethodData { get; }
        public MethodArgumentInfo ArgumentInfo { get; set; }
    }

    internal class PropertyProfilerContext : ProfilerContext
    {
        public PropertyProfilerContext(
            PropertyData propertyData,
            string sourceFileName,
            int lineNumber,
            int warmupCount,
            int iterationCount,
            Runtime runtime,
            TimeUnit baseUnit,
            Action<ProfilerBatchResult, string> logger,
            Func<ProfilerBatchResult, string, Task> asyncLogger) : base(sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger)
        {
            this.PropertyData = propertyData;
        }

        public PropertyData PropertyData { get; }
    }

    internal class ConstructorProfilerContext : ProfilerContext
    {
        public ConstructorProfilerContext(
            ConstructorData constructorData,
            string sourceFileName,
            int lineNumber,
            int warmupCount,
            int iterationCount,
            Runtime runtime,
            TimeUnit baseUnit,
            Action<ProfilerBatchResult, string> logger,
            Func<ProfilerBatchResult, string, Task> asyncLogger) : base(sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger)
        {
            this.ConstructorData = constructorData;
        }

        public ConstructorData ConstructorData { get; }
    }
}
