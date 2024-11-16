namespace BionicCode.Utilities.Net
{
  using System;

  internal abstract class RegistrationCommand<TEventSource> : IRegistrationCommand<TEventSource>
  {
    protected Delegate ClientHandler { get; }
    protected string EventName { get; }

    protected RegistrationCommand(Delegate clientHandler, string eventName)
    {
      this.ClientHandler = clientHandler;
      this.EventName = eventName;
    }

    public abstract void RegisterDelegate(TEventSource eventSource);
    public abstract void UnregisterDelegate(TEventSource eventSource);
  }
}