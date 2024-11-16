namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Collections;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Diagnostics.Tracing;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using System.Xml.Linq;

  internal class EventInfoTableEntry<TEventSource>
  {
    private bool? isStaticEvent;

    public EventInfoTableEntry(Delegate generatedHandler, EventInfo sourceEventInfo, object eventSource)
    {
      this.Handler = generatedHandler;
      this.EventInfo = sourceEventInfo;
      this.eventName = this.EventInfo.Name;
      this.RegistrationService = new WeakEventRegistrationService<TEventSource>();
      if (eventSource != null)
      {
        this.RegistrationService.AddSourceInstance(eventSource);
      }
    }

    public EventInfoTableEntry(TEventSource eventSource) : this(null, null, eventSource)
    {
      Debug.Assert(eventSource != null);
    }

    public EventInfoTableEntry(EventInfo eventInfo) : this(null, eventInfo, null)
    {
      Debug.Assert(eventInfo != null);
    }

    //public void AddRegistration(IClientEventHandlerRegistrar<TEventSource> registrationCommand)
    //  => this.RegistrationService.AddSubscribeDelegate(registrationDelegate);

    //public void Unregister(Action<object> registrationDelegate)
    //  => this.RegistrationService.RemoveRegistration(registrationDelegate);

    //public void AddEventSource(object eventSource)
    //  => this.RegistrationService.AddInstance(eventSource);

    //public void RemoveEventSource(object eventSource)
    //  => this.RegistrationService.RemoveInstance(eventSource);

    //public void RemoveAllEventSources()
    //  => this.RegistrationService.ClearInstances();

    public bool IsStaticEvent => this.EventInfo is null 
      ? throw new InvalidOperationException($"The {nameof(this.EventInfo)} property is NULL") 
      : (bool)(this.isStaticEvent ?? (this.isStaticEvent = this.EventInfo.GetAddMethod().IsStatic)); 

    public Delegate Handler { get; set; }
    public EventInfo EventInfo { get; set; }
    public MethodBase InvocatorMethod => this.EventInfo is null 
      ? throw new InvalidOperationException($"The property {nameof(this.EventInfo)} is NULL.") 
      : MethodInfo.GetMethodFromHandle((this.invocatorHandle ?? (this.invocatorHandle = this.EventInfo.EventHandlerType.GetMethod("invoke").MethodHandle)).Value);

    private RuntimeMethodHandle? invocatorHandle;
    private readonly string eventName;
    public string EventName => this.eventName ?? (this.EventInfo is null
      ? throw new InvalidOperationException($"The {nameof(this.EventInfo)} property is NULL")
      : this.EventInfo.Name);

    public WeakEventRegistrationService<TEventSource> RegistrationService { get; }
  }
}