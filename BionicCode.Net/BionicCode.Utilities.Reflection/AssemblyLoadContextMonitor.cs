namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading;

internal static class AssemblyLoadContextMonitor
{
    private static readonly ConditionalWeakTable<AssemblyLoadContext, AssemblyLoadContextInfoInternal> s_monitoredAssemblyLoadContextIdTable = new ConditionalWeakTable<AssemblyLoadContext, AssemblyLoadContextInfoInternal>();
    private static readonly ConcurrentDictionary<AssemblyLoadContextInfoInternal, AssemblyLoadContextInfo> s_assemblyLoadContextInfoTable = new ConcurrentDictionary<AssemblyLoadContextInfoInternal, AssemblyLoadContextInfo>();
    private static readonly ConcurrentHashSet<uint> s_unloadedAssemblyLoadContextIds = new ConcurrentHashSet<uint>();
    private static uint s_nextAssemblyLoadContextId;

    public static event EventHandler<AssemblyLoadContextUnloadingEventArgs> AssemblyLoadContextUnloading;

    internal sealed class AssemblyLoadContextUnloadingEventArgs
    {
        public AssemblyLoadContextInfo AssemblyLoadContextInfo { get; }

        public AssemblyLoadContextUnloadingEventArgs(AssemblyLoadContextInfo assemblyLoadContextInfo)
            => this.AssemblyLoadContextInfo = assemblyLoadContextInfo;
    }

    internal sealed class AssemblyLoadContextInfo
    {
        public AssemblyLoadContextInfo(AssemblyLoadContextInfoInternal assemblyLoadContextInfoInternal)
            => this._assemblyLoadContextInfoInternal = assemblyLoadContextInfoInternal;

        public uint Id
            => this._assemblyLoadContextInfoInternal.Id;

        public bool IsUnloading
            => this._assemblyLoadContextInfoInternal.IsUnloading;

        public object SyncLock { get; } = new object();

        private readonly AssemblyLoadContextInfoInternal _assemblyLoadContextInfoInternal;
    }

    internal sealed class AssemblyLoadContextInfoInternal
    {
        public AssemblyLoadContextInfoInternal(uint assemblyLoadContextId)
            => this.Id = assemblyLoadContextId;

        public void MarkAsUnloading()
            => _ = Interlocked.Exchange(ref this._isUnloadingFlag, 1);

        public uint Id { get; }

        private int _isUnloadingFlag;
        public bool IsUnloading
            => this._isUnloadingFlag == 1;
    }

    public static bool TryStartMonitoringAssembly(Assembly assembly, out AssemblyLoadContextInfo assemblyLoadContextInfo)
    {
        AssemblyLoadContext? assemblyLoadContext = AssemblyLoadContext.GetLoadContext(assembly);
        return AssemblyLoadContextMonitor.TryStartMonitoringAssembly(assemblyLoadContext, out assemblyLoadContextInfo);
    }

    public static bool TryStartMonitoringAssembly(AssemblyLoadContext? assemblyLoadContext, out AssemblyLoadContextInfo? assemblyLoadContextInfo)
    {
        assemblyLoadContextInfo = default;

        if (assemblyLoadContext is null
            || !assemblyLoadContext.IsCollectible)
        {
            return false;
        }

        AssemblyLoadContextInfoInternal assemblyLoadContextInfoInternal = AssemblyLoadContextMonitor.s_monitoredAssemblyLoadContextIdTable.GetValue(assemblyLoadContext, RegisterNewAssemblyLoadContext);

        // Condition is only true if the obtained 'assemblyLoadContextInfo' is already existing AND had been marked as unloading
        if (assemblyLoadContextInfo.IsUnloading)
        {
            // safety belt: throw or log and fail-fast
            throw new InvalidOperationException("ALC was already unloaded. Registering new assemblies for this ALC is not allowed.");
        }

        assemblyLoadContextInfo = AssemblyLoadContextMonitor.s_assemblyLoadContextInfoTable.GetOrAdd(assemblyLoadContextInfoInternal, internalInfo => new AssemblyLoadContextInfo(internalInfo));

        return true;
    }

    public static bool TryGetAssemblyLoadContextId(Assembly assembly, out AssemblyLoadContextInfo? assemblyLoadContextInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(assembly);

        assemblyLoadContextInfo = default;

        AssemblyLoadContext? assemblyLoadContext = AssemblyLoadContext.GetLoadContext(assembly) ?? throw new InvalidOperationException($"The assembly load context for assembly '{assembly.FullName}' could not be determined.");

        if (AssemblyLoadContextMonitor.s_monitoredAssemblyLoadContextIdTable.TryGetValue(assemblyLoadContext, out AssemblyLoadContextInfoInternal? assemblyLoadContextInfoInternal))
        {
            assemblyLoadContextInfo = AssemblyLoadContextMonitor.s_assemblyLoadContextInfoTable.GetOrAdd(assemblyLoadContextInfoInternal, internalInfo => new AssemblyLoadContextInfo(internalInfo));

            return true;
        }

        return false;
    }

    private static AssemblyLoadContextInfoInternal RegisterNewAssemblyLoadContext(AssemblyLoadContext assemblyLoadContext)
    {
        uint assemblyContextId = Interlocked.Increment(ref AssemblyLoadContextMonitor.s_nextAssemblyLoadContextId);

        // wrapped to 0 -> overflow of the 32-bit counter
        if (assemblyContextId == 0)
        {
            // safety belt: throw or log and fail-fast
            throw new InvalidOperationException("ALC id space exhausted (uint overflow). Switch to a larger id type (ulong).");
        }

        var assemblyLoadContextId = new AssemblyLoadContextInfoInternal(assemblyContextId);
        assemblyLoadContext!.Unloading += OnAssemblyLoadContextUnloading;

        return assemblyLoadContextId;
    }

    private static void OnAssemblyLoadContextUnloading(AssemblyLoadContext assemblyLoadContext)
    {
        if (AssemblyLoadContextMonitor.s_monitoredAssemblyLoadContextIdTable.TryGetValue(assemblyLoadContext, out AssemblyLoadContextInfoInternal? assemblyLoadContextInfoInternal))
        {
            assemblyLoadContextInfoInternal.MarkAsUnloading();

            // Remember the unloaded ALC ID
            _ = AssemblyLoadContextMonitor.s_unloadedAssemblyLoadContextIds.Add(assemblyLoadContextInfoInternal.Id);

            try
            {
                AssemblyLoadContextInfo assemblyLoadContextInfo = AssemblyLoadContextMonitor.s_assemblyLoadContextInfoTable.GetOrAdd(assemblyLoadContextInfoInternal, internalInfo => new AssemblyLoadContextInfo(internalInfo));
                AssemblyLoadContextMonitor.AssemblyLoadContextUnloading?.Invoke(null, new AssemblyLoadContextUnloadingEventArgs(assemblyLoadContextInfo));
            }
            catch (Exception)
            {
                // Swallow exceptions thrown by event subscribers to avoid unhandled exceptions during assembly unloading
                // and causing the AssemblyMonitor to stop (if a handler allows exceptions to propagate i.e. not catch the exception or rethrows it).
            }
        }
    }
}
