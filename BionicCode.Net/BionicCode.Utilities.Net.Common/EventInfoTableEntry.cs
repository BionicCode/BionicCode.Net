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
      this.EventSourceInstances = new WeakCollection<object>();
      if (eventSource != null)
      {
        this.EventSourceInstances.Add(eventSource);
      }
    }

    public EventInfoTableEntry(object eventSource) : this(null, null, eventSource)
    {
    }

    public bool IsStaticEvent => this.EventInfo is null 
      ? throw new InvalidOperationException($"The {nameof(this.EventInfo)} property is NULL") 
      : (bool)(this.isStaticEvent ?? (this.isStaticEvent = this.EventInfo.GetAddMethod().IsStatic)); 
    public Delegate Handler { get; set; }
    public EventInfo EventInfo { get; set; }
    public string EventName => this.EventInfo?.Name ?? string.Empty;
    public WeakCollection<object> EventSourceInstances { get; }
  }
}