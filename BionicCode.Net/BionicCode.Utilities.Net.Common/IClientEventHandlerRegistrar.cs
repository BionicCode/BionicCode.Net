namespace BionicCode.Utilities.Net
{
  using System;

  internal interface IClientEventHandlerRegistrar
  {
    string EventName { get; }
    Type EventSourceType { get; }
    void RegisterDelegate(object eventSource);
    void UnregisterDelegate(object eventSource);
    bool ContainsDelegate(Delegate handler);
    bool TryGetClientHandler(out Delegate clientHandler);
  }
}