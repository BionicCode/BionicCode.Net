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

    protected static readonly Dictionary<ManagedWeakTableKey, HashSet<ManagedWeakTableEntry>> ItemsInternal = new Dictionary<ManagedWeakTableKey, HashSet<ManagedWeakTableEntry>>();

    private static readonly TimeSpan PurgeInterval = TimeSpan.FromSeconds(10);
    internal static readonly object SyncLockInternal = new object();
    private static bool IsPurgeActive;
    private static Timer PurgeTimer;
    protected static ReaderWriterLockSlim TableLockInternal = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

    private static readonly object tableLock = new object();
    public static object TableLock => ManagedWeakTable.tableLock;

    public static bool HasEntry => ItemsInternal.Any();

    public static WeakReference<object> GetOrCreateWeakReference(object reference)
    {
      lock (ManagedWeakTable.SyncLockInternal)
      {
        WeakReference<object> weakReference = WeakReferencePool.GetOrCreate(reference);
        return weakReference;
      }
    }

    public static void RecycleWeakReference(WeakReference<object> weakReference)
      => WeakReferencePool.Add(weakReference);

    public static bool TryGetEntry(Guid eventSourceId, ManagedWeakTableKey key, out ManagedWeakTableEntry entry)
    {
      try
      {
        ManagedWeakTable.TableLockInternal.EnterReadLock();
        entry = null;
        if (ManagedWeakTable.ItemsInternal.TryGetValue(key, out HashSet<ManagedWeakTableEntry> entries))
        {
          foreach (ManagedWeakTableEntry tableEntry in entries)
          {
            if (tableEntry.Id.Equals(eventSourceId))
            {
              entry = tableEntry;
              return true;
            }
          }
        }

        return false;
      }
      finally
      {
        ManagedWeakTable.TableLockInternal.ExitReadLock();
      }
    }

    protected static void AddEntries(ManagedWeakTableKey key, IEnumerable<ManagedWeakTableEntry> entries)
    {
      try
      {
        ManagedWeakTable.TableLockInternal.EnterWriteLock();

        foreach (ManagedWeakTableEntry entry in entries)
        {
          AddEntryInternal(key, entry);
        }
      }
      finally
      {
        ManagedWeakTable.TableLockInternal.ExitWriteLock();
      }
    }

    protected static void AddEntry(ManagedWeakTableKey key, ManagedWeakTableEntry entry)
    {
      try
      {
        ManagedWeakTable.TableLockInternal.EnterWriteLock();

        AddEntryInternal(key, entry);
      }
      finally
      {
        ManagedWeakTable.TableLockInternal.ExitWriteLock();
      }
    }

    private static void AddEntryInternal(ManagedWeakTableKey key, ManagedWeakTableEntry entry)
    {
      ManagedWeakTable.Count++;

#if DEBUG
      Debug.WriteLine($"WeakEventManager instance #{(entry.TryGetReferenceTarget(out object referenceTarget) && referenceTarget is WeakEventManager manager ? manager.InstanceNumber : -1)} of {WeakEventManager.InstanceCounter}: -------- WeakTable ADD entry via API. Current entry count: {Count}.");
#endif

      if (!ManagedWeakTable.ItemsInternal.TryGetValue(key, out HashSet<ManagedWeakTableEntry> existingEntries))
      {
        existingEntries = new HashSet<ManagedWeakTableEntry>();
        ManagedWeakTable.ItemsInternal.Add(key, existingEntries);
      }

      _ = existingEntries.Add(entry);

      if (!ManagedWeakTable.IsPurgeActive && ManagedWeakTable.HasEntry)
      {
        Debug.WriteLine($"========= Purge timer started... =========");
        StartPurge();
      }
    }

    protected static bool RemoveEntry(ManagedWeakTableKey key, ManagedWeakTableEntry entry)
    {
      try
      {
        ManagedWeakTable.TableLockInternal.EnterWriteLock();

        bool hasRemovedItem = false;
        if (ManagedWeakTable.ItemsInternal.TryGetValue(key, out HashSet<ManagedWeakTableEntry> existingEntries))
        {
          ManagedWeakTable.Count--;

#if DEBUG
          Debug.WriteLine($"WeakEventManager instance #{(entry is WeakManagerTableEntry managerTableEntry ? managerTableEntry.WeakEventManagerInstanceNumber : -1)} of {WeakEventManager.InstanceCounter}: -------- WeakTable REMOVE entry via API. Current entry count: {Count}.");
#endif

          hasRemovedItem = existingEntries.Remove(entry);
          Debug.Assert(hasRemovedItem);

          _ = entry.TryPurge(isForced: true);

          if (!existingEntries.Any())
          {
            _ = ManagedWeakTable.ItemsInternal.Remove(key);
          }
        }

        return hasRemovedItem;
      }
      finally
      {
        ManagedWeakTable.TableLockInternal.ExitWriteLock();
      }
    }

    private static void OnPurgeTimerElapsed(object state)
    {
      try
      {
        ManagedWeakTable.TableLockInternal.EnterUpgradeableReadLock();

        var internalItems = ManagedWeakTable.ItemsInternal.ToList();
        for (int entryIndex = ManagedWeakTable.ItemsInternal.Count - 1; entryIndex >= 0; entryIndex--)
        {
          KeyValuePair<ManagedWeakTableKey, HashSet<ManagedWeakTableEntry>> internalItemsEntry = internalItems[entryIndex];
          HashSet<ManagedWeakTableEntry> managedTableEntries = internalItemsEntry.Value;
          foreach (ManagedWeakTableEntry managedWeakTableEntry in managedTableEntries)
          {
            if (managedWeakTableEntry.TryPurge(isForced: false))
            {
              try
              {
                ManagedWeakTable.TableLockInternal.EnterWriteLock();

                bool isRemoved = ManagedWeakTable.ItemsInternal[internalItemsEntry.Key].Remove(managedWeakTableEntry);
                Debug.Assert(isRemoved);

#if DEBUG
                Debug.WriteLine($"WeakEventManager instance #{(managedWeakTableEntry is WeakManagerTableEntry managerTableEntry ? managerTableEntry.WeakEventManagerInstanceNumber : -1)} of {WeakEventManager.InstanceCounter}: -------- WeakTable PURGED entry via purge timer. Current entry count: {Count}.");
#endif
              }
              finally
              {
                ManagedWeakTable.TableLockInternal.ExitWriteLock();
              }
            }
          }

          if (!managedTableEntries.Any())
          {
            try
            {
              ManagedWeakTable.TableLockInternal.EnterWriteLock();

              Debug.WriteLine($"========= Purge completed... =========");
              _ = ManagedWeakTable.ItemsInternal.Remove(internalItemsEntry.Key);
            }
            finally
            {
              ManagedWeakTable.TableLockInternal.ExitWriteLock();
            }
          }
        }

        if (!HasEntry)
        {
          Debug.WriteLine($"========= Purge timer stopped... =========");
          StopPurge();
        }
      }
      finally
      {
        ManagedWeakTable.TableLockInternal.ExitUpgradeableReadLock();
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