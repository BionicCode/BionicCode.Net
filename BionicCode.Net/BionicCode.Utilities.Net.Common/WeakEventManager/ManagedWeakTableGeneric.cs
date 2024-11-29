namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;

  internal abstract partial class ManagedWeakTable<TEntry> : ManagedWeakTable where TEntry : ManagedWeakTableEntry
  {
    protected static bool TryGetEntry<TEventSource>(ManagedWeakTableKey key, object eventSource, out EntryInfo<TEntry> entryInfo)
    {
      entryInfo = default;

      lock (ManagedWeakTable.SyncLockInternal)
      {
        if (!ManagedWeakTable.ItemsInternal.TryGetValue(key, out HashSet<ManagedWeakTableEntry> entries))
        {
          return false;
        }

        var tableEntries = entries.ToList();
        for (int entryIndex = 0; entryIndex < tableEntries.Count; entryIndex++)
        {
          ManagedWeakTableEntry managedWeakTableEntry = tableEntries[entryIndex];
          if (!(managedWeakTableEntry is TEntry tableEntry))
          {
            continue;
          }

          if (tableEntry.ReferenceTarget.TryGetTarget(out object entryEventSource))
          {
            if (ReferenceEquals(entryEventSource, eventSource))
            {
              entryInfo = new EntryInfo<TEntry>(tableEntry, entries);
              return true;
            }
          }
          else
          {
            // ManagedReference was garbage collected and is therefore eligible for recycling
            Debug.WriteLine("Failed to get entry because entry is expired ==> Recycle ");
            if (tableEntry is IPurgeable purgeableEntry)
            {
              _ = purgeableEntry.TryPurge(isForced: true);
            }
            else
            {
              tableEntry.Recycle();
            }

            bool isRemoved = entries.Remove(managedWeakTableEntry);
            Debug.Assert(isRemoved);

            if (!entries.Any())
            {
              isRemoved = ManagedWeakTable.ItemsInternal.Remove(key);
              Debug.Assert(isRemoved);
            }
          }
        }
      }

      return false;
    }
  }
}