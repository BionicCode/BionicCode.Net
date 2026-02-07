namespace BionicCode.Utilities.Net
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;

    internal abstract class ManagedWeakTable<TEntry> : ManagedWeakTable where TEntry : ManagedWeakTableEntry
    {
        protected static bool TryGetEntry(ManagedWeakTableKey key, object eventSource, out EntryInfo<TEntry> entryInfo)
        {
            entryInfo = default;

            try
            {
                ManagedWeakTable.TableLockInternal.EnterUpgradeableReadLock();

                if (!ManagedWeakTable.ItemsInternal.TryGetValue(key, out HashSet<ManagedWeakTableEntry> entries))
                {
                    return false;
                }

                var tableEntries = entries.ToImmutableHashSet();
                foreach (ManagedWeakTableEntry managedWeakTableEntry in tableEntries)
                {
                    if (managedWeakTableEntry is not TEntry tableEntry)
                    {
                        continue;
                    }

                    if (tableEntry.TryGetReferenceTarget(out object referenceTarget))
                    {
                        if (ReferenceEquals(referenceTarget, eventSource))
                        {
                            entryInfo = new EntryInfo<TEntry>(tableEntry, tableEntries);
                            return true;
                        }
                    }
                    else
                    {
                        try
                        {
                            ManagedWeakTable.TableLockInternal.EnterWriteLock();

                            // ManagedReference was garbage collected and is therefore eligible for recycling
                            Debug.WriteLine($"Failed to get entry for event '{key.ReferenceTargetId}' because entry is expired ==> Purge and Recycle ");
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
                        finally
                        {
                            ManagedWeakTable.TableLockInternal.ExitWriteLock();
                        }
                    }
                }

                return false;
            }
            finally
            {
                ManagedWeakTable.TableLockInternal.ExitUpgradeableReadLock();
            }
        }
    }
}
