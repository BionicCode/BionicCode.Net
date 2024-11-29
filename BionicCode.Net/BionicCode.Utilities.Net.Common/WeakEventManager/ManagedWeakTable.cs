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
    protected static ReaderWriterLockSlim TableLock { get; } = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

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

    protected static void AddEntries(ManagedWeakTableKey key, IEnumerable<ManagedWeakTableEntry> entries)
    {
      try
      {
        ManagedWeakTable.TableLock.EnterWriteLock();

        foreach (ManagedWeakTableEntry entry in entries)
        {
          AddEntryInternal(key, entry);
        }
      }
      finally
      {
        ManagedWeakTable.TableLock.ExitWriteLock();
      }
    }

    protected static void AddEntry(ManagedWeakTableKey key, ManagedWeakTableEntry entry)
    {
      try
      {
        ManagedWeakTable.TableLock.EnterWriteLock();

        AddEntryInternal(key, entry);
      }
      finally
      {
        ManagedWeakTable.TableLock.ExitWriteLock();
      }
    }

    private static void AddEntryInternal(ManagedWeakTableKey key, ManagedWeakTableEntry entry)
    {
      ManagedWeakTable.Count++;
      Debug.WriteLine($"-------- WeakTable add entry via API. Current entry count: {Count}");
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
        ManagedWeakTable.TableLock.EnterWriteLock();

        bool hasRemovedItem = false;
        if (ManagedWeakTable.ItemsInternal.TryGetValue(key, out HashSet<ManagedWeakTableEntry> existingEntries))
        {
          ManagedWeakTable.Count--;
          Debug.WriteLine($"-------- WeakTable remove entry via API. Current entry count: {Count}");

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
        ManagedWeakTable.TableLock.ExitWriteLock();
      }
    }

    private static void OnPurgeTimerElapsed(object state)
    {
      try
      {
        ManagedWeakTable.TableLock.EnterUpgradeableReadLock();

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
                ManagedWeakTable.TableLock.EnterWriteLock();

                bool isRemoved = ManagedWeakTable.ItemsInternal[internalItemsEntry.Key].Remove(managedWeakTableEntry);
                Debug.Assert(isRemoved);
                Debug.WriteLine($"Purged... {managedWeakTableEntry.GetType().Name}. Is removed from table: {isRemoved}");
              }
              finally
              {
                ManagedWeakTable.TableLock.ExitWriteLock();
              }
            }
          }

          if (!managedTableEntries.Any())
          {
            try
            {
              ManagedWeakTable.TableLock.EnterWriteLock();

              Debug.WriteLine($"========= Purge completed... =========");
              _ = ManagedWeakTable.ItemsInternal.Remove(internalItemsEntry.Key);
            }
            finally
            {
              ManagedWeakTable.TableLock.ExitWriteLock();
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
        ManagedWeakTable.TableLock.ExitUpgradeableReadLock();
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