namespace BionicCode.Utilities.Net
{
  using System;
  using System.Diagnostics;

  /// <summary>
  /// Stores one WeakEventManger per event source instance and per event name
  /// </summary>
  internal sealed class WeakEventManagerTable : ManagedWeakTable<WeakManagerTableEntry>
  {
    public static WeakEventManager<TEventSource> GetOrCreateWeakEventManager<TEventSource>(object eventSource, string eventName, bool isCustomClientDelegate)
    {
      WeakEventManager<TEventSource> weakEventManager;
      Type eventSourceType = typeof(TEventSource);
      var key = new ManagedWeakTableKey(eventName, eventSourceType);

      // If the event is a static event, the eventSource is NULL.
      if (!ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry<TEventSource>(key, eventSource, out EntryInfo<WeakManagerTableEntry> entryInfo)
        || entryInfo.Entry.IsPurged)
      {
        if (entryInfo?.Entry.IsPurged ?? false)
        {
          bool hasRemoved = ManagedWeakTable.RemoveEntry(key, entryInfo.Entry);
          Debug.Assert(hasRemoved);
        }

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

    public static bool TryGetWeakEventManager<TEventSource>(object eventSource, string eventName, out WeakEventManager<TEventSource> weakEventManager)
    {
      weakEventManager = null;
      Type eventSourceType = typeof(TEventSource);
      var key = new ManagedWeakTableKey(eventName, eventSourceType);
      // If the event is a static event, the eventSource is NULL.
      if (ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry<TEventSource>(key, eventSource, out EntryInfo<WeakManagerTableEntry> entryInfo))
      {
        weakEventManager = (WeakEventManager<TEventSource>)entryInfo.Entry.WeakEventManager;
      }

      return weakEventManager != null;
    }

    public static void RemoveWeakEventManager<TEventSource>(object eventSource, string eventName)
    {
      Debug.WriteLine("RemoveWeakEventManager API call");
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