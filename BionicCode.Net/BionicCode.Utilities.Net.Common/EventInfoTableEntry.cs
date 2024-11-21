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

  internal class EventInfoTableEntry
  {
    public EventInfoTableEntry(EventData eventData, Delegate eventHandler)
    {
      this.EventData = eventData;
      this.EventHandler = eventHandler;
    }

    public EventData EventData { get; }
    public Delegate EventHandler { get; }
  }
}