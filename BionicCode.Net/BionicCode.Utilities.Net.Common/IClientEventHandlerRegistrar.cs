namespace BionicCode.Utilities.Net
{
  using System;

  internal interface IClientEventHandlerRegistrar<TEventSource>
  {
    void RegisterDelegate(TEventSource eventSource);
    void UnregisterDelegate(TEventSource eventSource);
    bool ContainsDelegate(Delegate handler);
  }
}