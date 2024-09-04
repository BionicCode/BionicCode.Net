namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  internal class EntryInfo<TEntry> where TEntry : ManagedWeakTableEntry
  {
    public EntryInfo(TEntry entry, HashSet<ManagedWeakTableEntry> bucket)
    {
      this.Entry = entry;
      this.Bucket = bucket;
    }

    public TEntry Entry { get; }
    public HashSet<ManagedWeakTableEntry> Bucket { get; }
  }
}