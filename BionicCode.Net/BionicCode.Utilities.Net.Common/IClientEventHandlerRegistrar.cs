namespace BionicCode.Utilities.Net
{
  using System;

  internal interface IClientEventHandlerRegistrar
  {
    string EventName { get; }
    void RegisterDelegate(object eventSource);
    void UnregisterDelegate(object eventSource);
    bool ContainsDelegate(Delegate handler);
  }
}