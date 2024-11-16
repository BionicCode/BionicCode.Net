namespace BionicCode.Utilities.Net
{
  internal interface IRegistrationCommand<TEventSource>
  {
    void RegisterDelegate(TEventSource eventSource);
    void UnregisterDelegate(TEventSource eventSource);
  }
}