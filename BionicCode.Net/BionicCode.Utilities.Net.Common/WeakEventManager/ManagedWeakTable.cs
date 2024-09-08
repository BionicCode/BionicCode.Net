namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Threading;

  internal abstract class ManagedWeakTable
  {
    internal static int Count;

    protected static readonly Dictionary<Type, HashSet<ManagedWeakTableEntry>> ItemsInternal = new Dictionary<Type, HashSet<ManagedWeakTableEntry>>();

    private static readonly Queue<WeakReference<object>> WeakReferencePool = new Queue<WeakReference<object>>();
    private static readonly TimeSpan PurgeInterval = TimeSpan.FromSeconds(10);
    internal static readonly object SyncLockInternal = new object();
    private static bool IsPurgeActive;
    private static Timer PurgeTimer;

    public static bool HasEntry => ItemsInternal.Any();

    public static WeakReference<object> GetOrCreateWeakReference(object reference)
    {
      lock (ManagedWeakTable.SyncLockInternal)
      {
        WeakReference<object> weakReference;
        if (ManagedWeakTable.WeakReferencePool.Any())
        {
          weakReference = ManagedWeakTable.WeakReferencePool.Dequeue();
          weakReference.SetTarget(reference);
        }
        else
        {
          weakReference = new WeakReference<object>(reference, trackResurrection: false);
        }

        return weakReference;
      }
    }

    public static void RecycleWeakReference(WeakReference<object> weakReference)
    {
      weakReference.SetTarget(null);
      ManagedWeakTable.WeakReferencePool.Enqueue(weakReference);
    }

    protected static void AddEntries(IEnumerable<ManagedWeakTableEntry> entries)
    {
      foreach (ManagedWeakTableEntry entry in entries)
      {
        AddEntry(entry);
      }
    }

    protected static void AddEntry(ManagedWeakTableEntry entry)
    {
      lock (ManagedWeakTable.SyncLockInternal)
      {
        ManagedWeakTable.Count++;
        Debug.WriteLine($"-------- WeakTable add entry via API. Current entry count: {Count}");
        if (ManagedWeakTable.ItemsInternal.TryGetValue(entry.EventSourceType, out HashSet<ManagedWeakTableEntry> existingEntries))
        {
          _ = existingEntries.Add(entry);
        }
        else
        {
          existingEntries = new HashSet<ManagedWeakTableEntry>() { entry };
          ManagedWeakTable.ItemsInternal.Add(entry.EventSourceType, existingEntries);
        }

        if (!ManagedWeakTable.IsPurgeActive && ManagedWeakTable.HasEntry)
        {
          Debug.WriteLine($"========= Purge timer started... =========");
          StartPurge();
        }
      }
    }

    protected static bool RemoveEntry(ManagedWeakTableEntry entry)
    {
      lock (ManagedWeakTable.SyncLockInternal)
      {
        bool hasRemovedItem = false;

        if (ManagedWeakTable.ItemsInternal.TryGetValue(entry.EventSourceType, out HashSet<ManagedWeakTableEntry> existingEntries))
        {
          ManagedWeakTable.Count--;
          Debug.WriteLine($"-------- WeakTable remove entry via API. Current entry count: {Count}");

          hasRemovedItem = existingEntries.Remove(entry);
          Debug.Assert(hasRemovedItem);

          _ = entry.TryPurge(isForced: true);

          if (!existingEntries.Any())
          {
            _ = ManagedWeakTable.ItemsInternal.Remove(entry.EventSourceType);
          }
        }

        return hasRemovedItem;
      }
    }

    private static void OnPurgeTimerElapsed(object state)
    {
      lock (ManagedWeakTable.SyncLockInternal)
      {
        var internalItems = ManagedWeakTable.ItemsInternal.ToList();
        for (int entryIndex = ManagedWeakTable.ItemsInternal.Count - 1; entryIndex >= 0; entryIndex--)
        {
          KeyValuePair<Type, HashSet<ManagedWeakTableEntry>> internalItemsEntry = internalItems[entryIndex];
          HashSet<ManagedWeakTableEntry> managedTableEntries = internalItemsEntry.Value;
          foreach (ManagedWeakTableEntry managedWeakTableEntry in managedTableEntries)
          {
            if (managedWeakTableEntry.TryPurge(isForced: false))
            {
              bool isRemoved = ManagedWeakTable.ItemsInternal[internalItemsEntry.Key].Remove(managedWeakTableEntry);
              Debug.Assert(isRemoved);
              Debug.WriteLine($"Purged... {managedWeakTableEntry.GetType().Name}. Is removed from table: {isRemoved}");
            }
          }

          if (!managedTableEntries.Any())
          {
            Debug.WriteLine($"========= Purge completed... =========");
            _ = ManagedWeakTable.ItemsInternal.Remove(internalItemsEntry.Key);
          }
        }

        if (!HasEntry)
        {
          Debug.WriteLine($"========= Purge timer stopped... =========");
          StopPurge();
        }
      }
    }

    private static void StartPurge()
    {
      ManagedWeakTable.PurgeTimer = new Timer(OnPurgeTimerElapsed, null, ManagedWeakTable.PurgeInterval, ManagedWeakTable.PurgeInterval);
      ManagedWeakTable.IsPurgeActive = true;
    }

    private static void StopPurge()
    {
      _ = ManagedWeakTable.PurgeTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
      ManagedWeakTable.IsPurgeActive = false;
      ManagedWeakTable.PurgeTimer?.Dispose();
      ManagedWeakTable.PurgeTimer = null;
    }
  }
}