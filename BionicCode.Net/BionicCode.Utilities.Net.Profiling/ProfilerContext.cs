namespace BionicCode.Utilities.Net.Profiling
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    // TODO::Move context related properties from ProfilerBatchResult to Context class and compose
    internal abstract class ProfilerContext
    {
        protected ProfilerContext(
            TypeData? targetInstanceTypeData,
            string sourceFileName,
            int lineNumber,
            int warmupCount,
            int iterationCount,
            Runtime runtime,
            TimeUnit baseUnit,
            Action<ProfilerBatchResult, string>? logger,
            Func<ProfilerBatchResult, string, Task>? asyncLogger,
            string shortMemberDisplayName)
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
            this.ShortMemberDisplayName = shortMemberDisplayName;
        }

        public Runtime Runtime { get; }
        public TypeData? TargetInstanceTypeData { get; }
        public string FullSourceFileName { get; }
        public string SourceFileName => Path.GetFileName(this.FullSourceFileName);
        public int LineNumber { get; }
        public string ShortMemberDisplayName { get; }
        public Lazy<string> RuntimeVersionFactory { get; }
        public string RuntimeVersion => this.RuntimeVersionFactory.Value;
        public int WarmupCount { get; }
        public int IterationCount { get; }
        public TimeUnit BaseUnit { get; }
        public Action<ProfilerBatchResult, string>? Logger { get; }
        public Func<ProfilerBatchResult, string, Task>? AsyncLogger { get; }
        public ProfiledTargetType
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
            Action<ProfilerBatchResult, string>? logger,
            Func<ProfilerBatchResult, string, Task>? asyncLogger,
            string shortMemberDisplayName) : base(typeof(TTarget).ToTypeData(), sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger, shortMemberDisplayName)
        {
            this.TargetInstance = targetInstance;
        }

        public TTarget TargetInstance { get; }
    }

    internal class ScopeProfilerContext : ProfilerContext
    {
        public ScopeProfilerContext(TypeData? targetInstanceTypeData, string sourceFileName, int lineNumber, int warmupCount, int iterationCount, Runtime runtime, TimeUnit baseUnit, Action<ProfilerBatchResult, string>? logger, Func<ProfilerBatchResult, string, Task>? asyncLogger)
            : base(targetInstanceTypeData, sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger, string.Empty)
        {
        }
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
            Action<ProfilerBatchResult, string>? logger,
            Func<ProfilerBatchResult, string, Task>? asyncLogger) : base(targetInstance, sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger, methodData.ShortDisplayName)
        {
            ArgumentExceptionAdvanced.ThrowIfTrue(
                methodData.IsOpenGenericMethodOrGenericMethodDefinition,
                nameof(methodData),
                "Open generic methods or generic method definitions are not supported for profiling.");
            this.MethodData = methodData;
        }

        public MethodData MethodData { get; }
        public MethodArgumentInfo ArgumentInfo { get; set; }
    }

    internal class PropertyProfilerContext<TTarget> : ProfilerContext<TTarget>
    {
        private PropertyArgumentInfo _argumentInfo;

        public PropertyProfilerContext(
            TTarget targetInstance,
            PropertyData propertyData,
            string sourceFileName,
            int lineNumber,
            int warmupCount,
            int iterationCount,
            Runtime runtime,
            TimeUnit baseUnit,
            Action<ProfilerBatchResult, string>? logger,
            Func<ProfilerBatchResult, string, Task>? asyncLogger) : base(targetInstance, sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger, propertyData.ShortDisplayName)
        {
            ArgumentNullException.ThrowIfNull(propertyData);
            ArgumentExceptionAdvanced.ThrowIfTrue(
                (this.ArgumentInfo.Accessor & PropertyAccessor.Set) != 0 && !propertyData.CanWrite,
                nameof(propertyData),
                $"The provided '{nameof(this.ArgumentInfo)}' does not match the provided argument '{nameof(propertyData)}'. The property '{propertyData.FullyQualifiedSignature}' is read-only but the '{nameof(this.ArgumentInfo)}' specifies a setter as profiling target.");
            ArgumentExceptionAdvanced.ThrowIfTrue(
                (this.ArgumentInfo.Accessor & PropertyAccessor.Get) != 0 && !propertyData.CanRead,
                nameof(propertyData),
                $"The provided '{nameof(this.ArgumentInfo)}' does not match the provided argument '{nameof(propertyData)}'. The property '{propertyData.FullyQualifiedSignature}' is write-only but the '{nameof(this.ArgumentInfo)}' specifies a getter as profiling target.");

            this.PropertyData = propertyData;
        }

        public PropertyData PropertyData { get; }
        public PropertyArgumentInfo ArgumentInfo
        {
            get => this._argumentInfo;
            set
            {
                this._argumentInfo = value;
                ArgumentExceptionAdvanced.ThrowIfTrue(
                    this.PropertyData.IsIndexer && !this._argumentInfo.IsForIndexer,
                    nameof(value),
                    $"Invalid value for '{typeof(PropertyProfilerContext<TTarget>).ToFullyQualifiedSignatureName}.{nameof(this.ArgumentInfo)}'. The property is an indexer but the argument info is not for an indexer.");
                ArgumentExceptionAdvanced.ThrowIfTrue(
                    !this.PropertyData.IsIndexer && this._argumentInfo.IsForIndexer,
                    nameof(value),
                    $"Invalid value for '{typeof(PropertyProfilerContext<TTarget>).ToFullyQualifiedSignatureName}.{nameof(this.ArgumentInfo)}'. The property is not an indexer but the argument info is for an indexer.");
            }
        }
        public bool IsProfilingGetter { get; set; }
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
            Action<ProfilerBatchResult, string>? logger,
            Func<ProfilerBatchResult, string, Task>? asyncLogger) : base(null, sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger, constructorData.ShortDisplayName)
        {
            ArgumentNullException.ThrowIfNull(constructorData);

            this.ConstructorData = constructorData;
        }

        public ConstructorData ConstructorData { get; }
        public MethodArgumentInfo ArgumentInfo { get; set; }
    }
}
