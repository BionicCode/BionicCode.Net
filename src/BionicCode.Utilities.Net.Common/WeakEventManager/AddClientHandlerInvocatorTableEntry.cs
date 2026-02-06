namespace BionicCode.Utilities.Net
{
    using System;
    using System.Threading;

    internal class AddClientHandlerInvocatorTableEntry
    {
        private readonly Delegate addHandlerInvocator;

        public AddClientHandlerInvocatorTableEntry(Delegate addHandlerInvocator, bool useAddCustomHandlerMethod, Type eventSourceType)
        {
            addHandlerInvocator = addHandlerInvocator;
            UseAddCustomHandlerMethod = useAddCustomHandlerMethod;
            EventSourceType = eventSourceType;
        }

        public Action<TEventSource, string, Delegate, SynchronizationContext> GetAddHandlerInvocator<TEventSource>()
          => typeof(TEventSource) != EventSourceType
            ? throw new ArgumentException($"ParameterType mismatch for generic type argument {nameof(TEventSource)}. Expected: {EventSourceType.FullName}; Found: {typeof(TEventSource).FullName}.")
            : (Action<TEventSource, string, Delegate, SynchronizationContext>)addHandlerInvocator;

        public bool UseAddCustomHandlerMethod { get; }
        public Type EventSourceType { get; }
    }
}
