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
            SymbolInfoData? symbolInfoData,
            string sourceFileName,
            int lineNumber,
            int warmupCount,
            int iterationCount,
            Runtime runtime,
            TimeUnit baseUnit,
            Action<ProfilerBatchResult, string>? logger,
            Func<ProfilerBatchResult, string, Task>? asyncLogger)
        {
            TargetInstanceTypeData = targetInstanceTypeData;
            SymbolInfoData = symbolInfoData;
            FullSourceFileName = sourceFileName;
            LineNumber = lineNumber;
            RuntimeVersionFactory = new Lazy<string>(() => System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
            WarmupCount = warmupCount;
            IterationCount = iterationCount;
            Runtime = runtime;
            BaseUnit = baseUnit;
            Logger = logger;
            AsyncLogger = asyncLogger;
        }

        public Runtime Runtime { get; }
        public TypeData? TargetInstanceTypeData { get; }
        public string FullSourceFileName { get; }
        public string SourceFileName => Path.GetFileName(FullSourceFileName);
        public int LineNumber { get; }
        public Lazy<string> RuntimeVersionFactory { get; }
        public string RuntimeVersion => RuntimeVersionFactory.Value;
        public int WarmupCount { get; }
        public int IterationCount { get; }
        public TimeUnit BaseUnit { get; }
        public Action<ProfilerBatchResult, string>? Logger { get; }
        public Func<ProfilerBatchResult, string, Task>? AsyncLogger { get; }
        public SymbolInfoData? SymbolInfoData { get; }
    }

    internal abstract class ProfilerContext<TTarget> : ProfilerContext
    {
        protected ProfilerContext(
            TTarget targetInstance,
            SymbolInfoData? symbolInfoData,
            string sourceFileName,
            int lineNumber,
            int warmupCount,
            int iterationCount,
            Runtime runtime,
            TimeUnit baseUnit,
            Action<ProfilerBatchResult, string>? logger,
            Func<ProfilerBatchResult, string, Task>? asyncLogger) : base(typeof(TTarget).ToTypeData(), symbolInfoData, sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger)
        {
            TargetInstance = targetInstance;
        }

        public TTarget TargetInstance { get; }
    }

    internal class ScopeProfilerContext : ProfilerContext
    {
        public ScopeProfilerContext(TypeData? targetInstanceTypeData, string sourceFileName, int lineNumber, int warmupCount, int iterationCount, Runtime runtime, TimeUnit baseUnit, Action<ProfilerBatchResult, string>? logger, Func<ProfilerBatchResult, string, Task>? asyncLogger)
            : base(targetInstanceTypeData, null, sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger)
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
            Func<ProfilerBatchResult, string, Task>? asyncLogger) : base(targetInstance, methodData, sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger)
        {
            ArgumentExceptionAdvanced.ThrowIfTrue(
                methodData.IsOpenGenericMethodOrGenericMethodDefinition,
                nameof(methodData),
                "Open generic methods or generic method definitions are not supported for profiling.");
        }

        public MethodData MethodData
            => (MethodData)SymbolInfoData!;

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
            Func<ProfilerBatchResult, string, Task>? asyncLogger) : base(targetInstance, propertyData, sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger)
        {
            ArgumentNullException.ThrowIfNull(propertyData);
            ArgumentExceptionAdvanced.ThrowIfTrue(
                (ArgumentInfo.Accessor & PropertyAccessor.Set) != 0 && !propertyData.CanWrite,
                nameof(propertyData),
                $"The provided '{nameof(ArgumentInfo)}' does not match the provided argument '{nameof(propertyData)}'. The property '{propertyData.FullyQualifiedSignature}' is read-only but the '{nameof(ArgumentInfo)}' specifies a setter as profiling target.");
            ArgumentExceptionAdvanced.ThrowIfTrue(
                (ArgumentInfo.Accessor & PropertyAccessor.Get) != 0 && !propertyData.CanRead,
                nameof(propertyData),
                $"The provided '{nameof(ArgumentInfo)}' does not match the provided argument '{nameof(propertyData)}'. The property '{propertyData.FullyQualifiedSignature}' is write-only but the '{nameof(ArgumentInfo)}' specifies a getter as profiling target.");
        }

        public PropertyData PropertyData
            => (PropertyData)SymbolInfoData!;

        public PropertyArgumentInfo ArgumentInfo
        {
            get => _argumentInfo;
            set
            {
                _argumentInfo = value;
                ArgumentExceptionAdvanced.ThrowIfTrue(
                    PropertyData.IsIndexer && !_argumentInfo.IsForIndexer,
                    nameof(value),
                    $"Invalid value for '{typeof(PropertyProfilerContext<TTarget>).ToFullyQualifiedSignatureName}.{nameof(ArgumentInfo)}'. The property is an indexer but the argument info is not for an indexer.");
                ArgumentExceptionAdvanced.ThrowIfTrue(
                    !PropertyData.IsIndexer && _argumentInfo.IsForIndexer,
                    nameof(value),
                    $"Invalid value for '{typeof(PropertyProfilerContext<TTarget>).ToFullyQualifiedSignatureName}.{nameof(ArgumentInfo)}'. The property is not an indexer but the argument info is for an indexer.");
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
            Func<ProfilerBatchResult, string, Task>? asyncLogger) : base(null, constructorData, sourceFileName, lineNumber, warmupCount, iterationCount, runtime, baseUnit, logger, asyncLogger)
        {
            ArgumentNullException.ThrowIfNull(constructorData);
        }

        public ConstructorData ConstructorData
            => (ConstructorData)SymbolInfoData!;

        public MethodArgumentInfo ArgumentInfo { get; set; }
    }
}
