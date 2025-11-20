namespace BionicCode.Utilities.Net
{
    using System;
    using System.Threading;

    internal class AddClientHandlerInvocatorTableEntry
    {
        private readonly Delegate addHandlerInvocator;

        public AddClientHandlerInvocatorTableEntry(Delegate addHandlerInvocator, bool useAddCustomHandlerMethod, Type eventSourceType)
        {
            this.addHandlerInvocator = addHandlerInvocator;
            this.UseAddCustomHandlerMethod = useAddCustomHandlerMethod;
            this.EventSourceType = eventSourceType;
        }

        public Action<TEventSource, string, Delegate, SynchronizationContext> GetAddHandlerInvocator<TEventSource>()
          => typeof(TEventSource) != this.EventSourceType
            ? throw new ArgumentException($"Type mismatch for generic type argument {nameof(TEventSource)}. Expected: {this.EventSourceType.FullName}; Found: {typeof(TEventSource).FullName}.")
            : (Action<TEventSource, string, Delegate, SynchronizationContext>)this.addHandlerInvocator;

        public bool UseAddCustomHandlerMethod { get; }
        public Type EventSourceType { get; }
    }
}
