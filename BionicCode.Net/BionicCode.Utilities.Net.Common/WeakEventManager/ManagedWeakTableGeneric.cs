namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;

  internal abstract partial class ManagedWeakTable<TEntry> : ManagedWeakTable where TEntry : ManagedWeakTableEntry
  {
    protected static bool TryGetEntry<TEventSource>(object eventSource, string eventName, out EntryInfo<TEntry> entryInfo)
    {
      entryInfo = default;

      lock (ManagedWeakTable.SyncLockInternal)
      {
        Type eventSourceType = typeof(TEventSource);
        if (!ManagedWeakTable.ItemsInternal.TryGetValue(eventSourceType, out HashSet<ManagedWeakTableEntry> entries))
        {
          return false;
        }

        Debug.Assert(ManagedWeakTable.ItemsInternal.Count(entry => entry.Key == eventSourceType) < 2);

        foreach (ManagedWeakTableEntry managedWeakTableEntry in entries)
        {
          if (!(managedWeakTableEntry is TEntry tableEntry))
          {
            continue;
          }

          if (tableEntry.EventSource.TryGetTarget(out object entryEventSource))
          {
            if (ReferenceEquals(entryEventSource, eventSource)
              && tableEntry.EventName.Equals(eventName, StringComparison.OrdinalIgnoreCase))
            {
              entryInfo = new EntryInfo<TEntry>(tableEntry, entries);
              return true;
            }
          }
          else
          {
            // ManagedReference was garbage collected and is therefore eligible for recycling
            tableEntry.Recycle();
          }
        }
      }

      return false;
    }
  }
}