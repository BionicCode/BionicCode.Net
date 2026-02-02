namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Loader;
    using System.Threading;

    internal static class AssemblyMonitor
    {
        private static readonly ConditionalWeakTable<AssemblyLoadContext, AssemblyLoadContextId> MonitoredAssemblyLoadContextIds = new ConditionalWeakTable<AssemblyLoadContext, AssemblyLoadContextId>();

        public static event EventHandler<AssemblyLoadContextUnloadingEventArgs>? AssemblyLoadContextUnloading;
        private static uint NextAssemblyLoadContextId;

        internal sealed class AssemblyLoadContextUnloadingEventArgs : EventArgs
        {
            public uint AssemblyLoadContextId { get; }

            public AssemblyLoadContextUnloadingEventArgs(uint assemblyLoadContextId)
                => this.AssemblyLoadContextId = assemblyLoadContextId;
        }

        internal sealed class AssemblyLoadContextId
        {
            public uint Id { get; }

            public AssemblyLoadContextId(uint assemblyLoadContextId)
                => this.Id = assemblyLoadContextId;
        }

        public static bool TryStartMonitoringAssembly(Assembly assembly, out uint assemblyContextId)
        {
            AssemblyLoadContext? assemblyLoadContext = AssemblyLoadContext.GetLoadContext(assembly);
            return AssemblyMonitor.TryStartMonitoringAssembly(assemblyLoadContext, out assemblyContextId);
        }

        public static bool TryStartMonitoringAssembly(AssemblyLoadContext? assemblyLoadContext, out uint assemblyContextId)
        {
            assemblyContextId = 0;

            if (assemblyLoadContext is null
                || !assemblyLoadContext.IsCollectible)
            {
                return false;
            }

            AssemblyLoadContextId assemblyLoadContextId = AssemblyMonitor.MonitoredAssemblyLoadContextIds.GetValue(assemblyLoadContext, RegisterNewAssemblyLoadContext);
            assemblyContextId = assemblyLoadContextId.Id;

            return true;
        }

        public static uint GetAssemblyLoadContextId(Assembly assembly)
        {
            AssemblyLoadContext? assemblyLoadContext = AssemblyLoadContext.GetLoadContext(assembly);
            if (assemblyLoadContext is null)
            {
                throw new InvalidOperationException($"The assembly load context for assembly '{assembly.FullName}' could not be determined.");
            }

            if (!AssemblyMonitor.MonitoredAssemblyLoadContextIds.TryGetValue(assemblyLoadContext, out AssemblyLoadContextId? assemblyLoadContextId))
            {
                throw new InvalidOperationException($"The assembly load context for assembly '{assembly.FullName}' is not being monitored.");
            }
            return assemblyLoadContextId.Id;
        }

        private static AssemblyLoadContextId RegisterNewAssemblyLoadContext(AssemblyLoadContext assemblyLoadContext)
        {
            uint assemblyContextId = Interlocked.Increment(ref AssemblyMonitor.NextAssemblyLoadContextId);

            // wrapped to 0 -> overflow of the 32-bit counter
            if (assemblyContextId == 0)
            {
                // safety belt: throw or log and fail-fast
                throw new InvalidOperationException("ALC id space exhausted (uint overflow). Switch to a larger id type (ulong).");
            }

            var assemblyLoadContextId = new AssemblyLoadContextId(assemblyContextId);
            assemblyLoadContext!.Unloading += OnAssemblyLoadContextUnloading;

            return assemblyLoadContextId;
        }

        private static void OnAssemblyLoadContextUnloading(AssemblyLoadContext assemblyLoadContext)
        {
            if (AssemblyMonitor.MonitoredAssemblyLoadContextIds.TryGetValue(assemblyLoadContext, out AssemblyLoadContextId? assemblyLoadContextId))
            {
                _ = AssemblyMonitor.MonitoredAssemblyLoadContextIds.Remove(assemblyLoadContext);

                try
                {
                    AssemblyMonitor.AssemblyLoadContextUnloading?.Invoke(null, new AssemblyLoadContextUnloadingEventArgs(assemblyLoadContextId.Id));
                }
                catch (Exception)
                {
                    // Swallow exceptions thrown by event subscribers to avoid unhandled exceptions during assembly unloading
                    // and causing the AssemblyMonitor to stop (if a handler allows exceptions to propagate i.e. not catch the exception or rethrows it).
                }
            }
        }
    }
}
