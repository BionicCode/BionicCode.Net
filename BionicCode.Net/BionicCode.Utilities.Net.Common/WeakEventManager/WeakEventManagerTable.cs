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
      if (!ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry(key, eventSource, out EntryInfo<WeakManagerTableEntry> entryInfo)
        || entryInfo.Entry.IsPurged)
      {
        if (entryInfo?.Entry.IsPurged ?? false)
        {
#if DEBUG
          int instanceNumber = entryInfo?.Entry.WeakEventManager.InstanceNumber ?? -1;
          Debug.WriteLine($"WeakEventManager instance #{instanceNumber} of {WeakEventManager.InstanceCounter}: Explicitly removing from weak table in GetOrCreateWeakEventManger().");
#endif
          bool hasRemoved = ManagedWeakTable.RemoveEntry(key, entryInfo.Entry);
          Debug.Assert(hasRemoved);
        }

        weakEventManager = new WeakEventManager<TEventSource>(eventName, isCustomClientDelegate);
        var tableEntry = new WeakManagerTableEntry(eventSource, typeof(TEventSource), eventName, weakEventManager);
        ManagedWeakTable.AddEntry(key, tableEntry);
#if DEBUG
        Debug.WriteLine($"WeakEventManager instance #{weakEventManager.InstanceNumber} of {WeakEventManager.InstanceCounter}: Created NEW from weak table in GetOrCreateWeakEventManger().");
#endif
      }
      else
      {
        weakEventManager = (WeakEventManager<TEventSource>)entryInfo.Entry.WeakEventManager;
#if DEBUG
        Debug.WriteLine($"WeakEventManager instance #{weakEventManager.InstanceNumber} of {WeakEventManager.InstanceCounter}: Returned EXISTING from weak table in GetOrCreateWeakEventManger().");
#endif
      }

      return weakEventManager;
    }

    public static bool TryGetWeakEventManager<TEventSource>(object eventSource, string eventName, out WeakEventManager<TEventSource> weakEventManager)
    {
      weakEventManager = null;
      Type eventSourceType = typeof(TEventSource);
      var key = new ManagedWeakTableKey(eventName, eventSourceType);
      // If the event is a static event, the eventSource is NULL.
      if (ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry(key, eventSource, out EntryInfo<WeakManagerTableEntry> entryInfo))
      {
        weakEventManager = (WeakEventManager<TEventSource>)entryInfo.Entry.WeakEventManager;

#if DEBUG
        Debug.WriteLine($"WeakEventManager instance #{weakEventManager.InstanceNumber} of {WeakEventManager.InstanceCounter}: Returned EXISTING from weak table in TryGetWeakEventManager().");
#endif
      }

      return weakEventManager != null;
    }

    public static void RemoveWeakEventManager<TEventSource>(Guid managerId, string eventName)
    {
      Debug.WriteLine("RemoveWeakEventManager API call");
      Type eventSourceType = typeof(TEventSource);
      var key = new ManagedWeakTableKey(eventName, eventSourceType);

      if (ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry(managerId, key, out ManagedWeakTableEntry entry))
      {
#if DEBUG
        Debug.WriteLine($"WeakEventManager instance #{(entry is WeakManagerTableEntry managerTableEntry ? managerTableEntry.WeakEventManagerInstanceNumber : -1)} of {WeakEventManager.InstanceCounter}: Removed manager from weak table in RemoveWeakEventManager().");
#endif

        _ = ManagedWeakTable.RemoveEntry(key, entry);
      }
    }
  }
}