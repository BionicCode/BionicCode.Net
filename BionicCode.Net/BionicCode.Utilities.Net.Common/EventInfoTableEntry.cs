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
  using System.Xml.Linq;

  internal class EventInfoTableEntry
  {
    private bool? isStaticEvent;

    public EventInfoTableEntry(Delegate generatedHandler, EventInfo sourceEventInfo, object eventSource)
    {
      this.Handler = generatedHandler;
      this.EventInfo = sourceEventInfo;
      this.eventName = this.EventInfo.Name;
      this.registrationService = new WeakEventRegistrationService();
      if (eventSource != null)
      {
        this.registrationService.AddInstance(eventSource);
      }
    }

    public EventInfoTableEntry(object eventSource) : this(null, null, eventSource)
    {
      Debug.Assert(eventSource != null);
    }

    public EventInfoTableEntry(EventInfo eventInfo) : this(null, eventInfo, null)
    {
      Debug.Assert(eventInfo != null);
    }

    public void AddRegistration(Action<object> registrationDelegate)
      => this.registrationService.AddSubscribeDelegate(registrationDelegate);

    public void RemoveRegistration(Action<object> registrationDelegate)
      => this.registrationService.RemoveRegistration(registrationDelegate);

    public void AddEventSource(object eventSource)
      => this.registrationService.AddInstance(eventSource);

    public void RemoveEventSource(object eventSource)
      => this.registrationService.RemoveInstance(eventSource);

    public void RemoveAllEventSources()
      => this.registrationService.ClearInstances();

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

    private readonly WeakEventRegistrationService registrationService;
  }

  internal class WeakEventRegistrationService
  {
    private readonly WeakCollection<object> eventSourceInstances;
    private readonly List<Action<object>> subscribeDelegates;
    private readonly List<Action<object>> unsubscribeDelegates;
    private readonly object syncLock;

    public WeakEventRegistrationService()
    {
      this.eventSourceInstances = new WeakCollection<object>();
      this.subscribeDelegates = new List<Action<object>>();
      this.unsubscribeDelegates = new List<Action<object>>();
      this.syncLock = new object();
    }

    public void AddInstance(object instance)
    {
      lock (this.syncLock)
      {
        this.eventSourceInstances.Add(instance);
        foreach (Action<object> subscribeDelegateDelegate in this.subscribeDelegates)
        {
          subscribeDelegateDelegate.Invoke(instance);
        }
      }
    }

    public void RemoveInstance(object instance)
    {
      lock (this.syncLock)
      {
        _ = this.eventSourceInstances.Remove(instance);
        foreach (Action<object> unsubscribeDelegateDelegate in this.unsubscribeDelegates)
        {
          unsubscribeDelegateDelegate.Invoke(instance);
        }
      }
    }

    public void AddSubscribeDelegate(Action<object> subscribeDelegate)
    {
      lock (this.syncLock)
      {
        this.subscribeDelegates.Add(subscribeDelegate);
        foreach (object instance in this.eventSourceInstances)
        {
          subscribeDelegate.Invoke(instance);
        }
      }
    }

      public void AddUnsubscribeDelegate(Action<object> unsubscribeDelegate)
      {

        throw new NotImplementedException();
      }

    public void RemoveRegistration(Action<object> instanceRegistrationDelegate)
    {
      lock (this.syncLock)
      {
        _ = this.subscribeDelegates.Remove(instanceRegistrationDelegate);
      }
    }

    internal void ClearInstances() 
      => this.eventSourceInstances.Clear();
  }
}