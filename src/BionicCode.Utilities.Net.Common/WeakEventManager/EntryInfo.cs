namespace BionicCode.Utilities.Net
{
    using System.Collections.Immutable;

    internal class EntryInfo<TEntry> where TEntry : ManagedWeakTableEntry
    {
        public EntryInfo(TEntry entry, ImmutableHashSet<ManagedWeakTableEntry> bucket)
        {
            Entry = entry;
            Bucket = bucket;
        }

        public TEntry Entry { get; }
        public ImmutableHashSet<ManagedWeakTableEntry> Bucket { get; }
    }
}
