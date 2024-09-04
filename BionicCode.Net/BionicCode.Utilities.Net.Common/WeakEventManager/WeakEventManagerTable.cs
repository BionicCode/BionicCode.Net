namespace BionicCode.Utilities.Net
{
  using System;

  internal sealed class WeakEventManagerTable : ManagedWeakTable<WeakManagerTableEntry>
  {
    public static WeakEventManager<TEventSource, TEventArgs> GetOrCreateWeakEventManager<TEventSource, TEventArgs>(object eventSource, string eventName)
      where TEventArgs : EventArgs
    {
      lock (ManagedWeakTable.SyncLockInternal)
      {
        WeakEventManager<TEventSource, TEventArgs> weakEventManager = null;
        if (!ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry<TEventSource>(eventSource, eventName, out EntryInfo<WeakManagerTableEntry> entryInfo))
        {
          weakEventManager = new WeakEventManager<TEventSource, TEventArgs>(eventName);
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
    public static bool TryGetWeakEventManager<TEventSource, TEventArgs>(object eventSource, string eventName, out WeakEventManager<TEventSource, TEventArgs> weakEventManager)
      where TEventArgs : EventArgs
    {
      weakEventManager = null;
      lock (ManagedWeakTable.SyncLockInternal)
      {
        if (ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry<TEventSource>(eventSource, eventName, out EntryInfo<WeakManagerTableEntry> entryInfo))
        {
          weakEventManager = (WeakEventManager<TEventSource, TEventArgs>)entryInfo.Entry.WeakEventManager;
        }

        return weakEventManager != null;
      }
    }

    public static void RemoveWeakEventManager<TEventSource>(object eventSource, string eventName)
    {
      lock (ManagedWeakTable.SyncLockInternal)
      {
        if (ManagedWeakTable<WeakManagerTableEntry>.TryGetEntry< TEventSource>(eventSource, eventName, out EntryInfo<WeakManagerTableEntry> entryInfo))
        {
          _ = ManagedWeakTable.RemoveEntry(entryInfo.Entry);
        }
      }
    }
  }
}