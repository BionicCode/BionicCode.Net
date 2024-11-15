namespace BionicCode.Utilities.Net
{
  using System;
  using System.Diagnostics;

  internal sealed class WeakEventManagerTable : ManagedWeakTable<WeakManagerTableEntry>
  {
    public static WeakEventManager<TEventSource, TEventArgs> GetOrCreateWeakEventManager<TEventSource, TEventArgs>(TEventSource eventSource, string eventName, System.Reflection.EventInfo eventInfo)
    {
      lock (ManagedWeakTable.SyncLockInternal)
      {
        WeakEventManager<TEventSource, TEventArgs> weakEventManager = null;

        // If the event is a static event, the eventSource is NULL.
        // In this case, we need to provide a placeholder for the WeakTable entry.
        if (!ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry(eventSource == null ? DummyEventSourceForStaticEventHandlers.Instance : (object)eventSource, eventName, out EntryInfo<WeakManagerTableEntry> entryInfo))
        {
          weakEventManager = new WeakEventManager<TEventSource, TEventArgs>(eventName, eventInfo);
          var tableEntry = new WeakManagerTableEntry(eventSource, typeof(TEventSource), eventName, weakEventManager);
          ManagedWeakTable.AddEntry(tableEntry);
        }
        else
        {
          weakEventManager = (WeakEventManager<TEventSource, TEventArgs>)entryInfo.Entry.WeakEventManager;
        }

        return weakEventManager;
      }
    }
    public static bool TryGetWeakEventManager<TEventSource, TEventArgs>(TEventSource eventSource, string eventName, out WeakEventManager<TEventSource, TEventArgs> weakEventManager)
    {
      weakEventManager = null;
      lock (ManagedWeakTable.SyncLockInternal)
      {
        // If the event is a static event, the eventSource is NULL.
        // In this case, we need to provide a placeholder for the WeakTable entry.
        if (ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry(eventSource == null ? DummyEventSourceForStaticEventHandlers.Instance : (object)eventSource, eventName, out EntryInfo<WeakManagerTableEntry> entryInfo))
        {
          weakEventManager = (WeakEventManager<TEventSource, TEventArgs>)entryInfo.Entry.WeakEventManager;
        }

        return weakEventManager != null;
      }
    }

    public static void RemoveWeakEventManager(object eventSource, string eventName)
    {
      Debug.WriteLine("RemoveWeakEventManager API call");
      lock (ManagedWeakTable.SyncLockInternal)
      {
        // If the event is a static event, the eventSource is NULL.
        // In this case, we need to provide a placeholder for the WeakTable entry.
        if (ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry(eventSource ?? DummyEventSourceForStaticEventHandlers.Instance, eventName, out EntryInfo<WeakManagerTableEntry> entryInfo))
        {
          _ = ManagedWeakTable.RemoveEntry(entryInfo.Entry);
        }
      }
    }
  }
}