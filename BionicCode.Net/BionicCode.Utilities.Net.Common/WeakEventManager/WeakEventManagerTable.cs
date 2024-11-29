namespace BionicCode.Utilities.Net
{
  using System;
  using System.Diagnostics;
  using System.Reflection;

  internal sealed class WeakEventManagerTable : ManagedWeakTable<WeakManagerTableEntry>
  {
    public static WeakEventManager<TEventSource> GetOrCreateWeakEventManager<TEventSource>(object eventSource, string eventName, bool isCustomClientDelegate)
    {
      lock (ManagedWeakTable.SyncLockInternal)
      {
        WeakEventManager<TEventSource> weakEventManager = null;
        Type eventSourceType = typeof(TEventSource);
        var key = new ManagedWeakTableKey(eventName, eventSourceType);

        // If the event is a static event, the eventSource is NULL.
        if (!ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry<TEventSource>(key, eventSource, out EntryInfo<WeakManagerTableEntry> entryInfo))
        {
          weakEventManager = new WeakEventManager<TEventSource>(eventName, isCustomClientDelegate);
          var tableEntry = new WeakManagerTableEntry(eventSource, typeof(TEventSource), eventName, weakEventManager);
          ManagedWeakTable.AddEntry(key, tableEntry);
        }
        else
        {
          weakEventManager = (WeakEventManager<TEventSource>)entryInfo.Entry.WeakEventManager;
        }

        return weakEventManager;
      }
    }

    public static bool TryGetWeakEventManager<TEventSource>(object eventSource, string eventName, out WeakEventManager<TEventSource> weakEventManager)
    {
      weakEventManager = null;
      lock (ManagedWeakTable.SyncLockInternal)
      {
        Type eventSourceType = typeof(TEventSource);
        var key = new ManagedWeakTableKey(eventName, eventSourceType);
        // If the event is a static event, the eventSource is NULL.
        if (ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry<TEventSource>(key, eventSource, out EntryInfo<WeakManagerTableEntry> entryInfo))
        {
          weakEventManager = (WeakEventManager<TEventSource>)entryInfo.Entry.WeakEventManager;
        }

        return weakEventManager != null;
      }
    }

    public static void RemoveWeakEventManager<TEventSource>(object eventSource, string eventName)
    {
      Debug.WriteLine("RemoveWeakEventManager API call");
      lock (ManagedWeakTable.SyncLockInternal)
      {
        Type eventSourceType = typeof(TEventSource);
        var key = new ManagedWeakTableKey(eventName, eventSourceType);

        // If the event is a static event, the eventSource is NULL.
        if (ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry<TEventSource>(key, eventSource, out EntryInfo<WeakManagerTableEntry> entryInfo))
        {
          _ = ManagedWeakTable.RemoveEntry(key, entryInfo.Entry);
        }
      }
    }
  }
}