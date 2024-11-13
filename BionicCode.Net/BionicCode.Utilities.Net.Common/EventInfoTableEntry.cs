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
    public EventInfoTableEntry(Delegate generatedHandler, EventInfo sourceEventInfo, object eventSource)
    {
      this.GeneratedHandler = generatedHandler;
      this.SourceEventInfo = sourceEventInfo;
      this.EventSourceInstances = new WeakCollection<object>() { eventSource };
    }

    public EventInfoTableEntry(object eventSource) : this(null, null, eventSource)
    {
    }

    public Delegate GeneratedHandler { get; }
    public EventInfo SourceEventInfo { get; }
    public WeakCollection<object> EventSourceInstances { get; }
  }
}