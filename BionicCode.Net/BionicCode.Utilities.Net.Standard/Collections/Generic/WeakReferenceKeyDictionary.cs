using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BionicCode.Utilities.Net.Standard.Collections.Generic
{
  public class WeakReferenceKeyDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : WeakReference
  {
    internal class Entry
    {
      public Entry(int hashCode, TKey key, TValue value) : this(hashCode, new KeyValuePair<TKey, TValue>(key, value))
      {
      }
      public Entry(int hashCode, KeyValuePair<TKey, TValue> keyValuePair)
      {
        this.Id = Guid.NewGuid();
        this.HashCode = hashCode;
        this.Key = keyValuePair.Key;
        this.Value = keyValuePair.Value;
        this.KeyValuePair = keyValuePair;
      }

      public int HashCode { get; set; }
      public TKey Key { get; set; }
      public TValue Value { get; set; }
      public KeyValuePair<TKey, TValue> KeyValuePair { get; set; }
      public Guid Id { get; set; }
    }

    public WeakReferenceKeyDictionary() : this(false, false)
    { }

    public WeakReferenceKeyDictionary(bool isReadOnly, bool isAutoPurgeFinalizedItemsEnabled)
    {
      this.IsReadOnly = isReadOnly;
      this.IsAutoPurgeFinalizedItemsEnabled = isAutoPurgeFinalizedItemsEnabled;
      this.TargetTable = new Dictionary<int, List<Entry>>();
      this.Entries = new Dictionary<TKey, Entry>();
    }

    public bool IsAutoPurgeFinalizedItemsEnabled { get; set; }
    private Dictionary<int, List<Entry>> TargetTable { get; }
    private Dictionary<TKey, Entry> Entries { get; }

    #region Implementation of IEnumerable

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      foreach (KeyValuePair<TKey, Entry> keyValuePair in this.Entries)
      {
        yield return keyValuePair.Value.KeyValuePair;
      }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region Implementation of ICollection<KeyValuePair<TKey,TValue>>

    /// <inheritdoc />
    public void Add(KeyValuePair<TKey, TValue> item)
    {
      if (this.IsReadOnly)
      {
        throw new NotSupportedException("Trying to modify a read-only collection.");
      }

      object target = item.Key.Target;
      if (target == null)
      {
        return;
      }
      int hashCode = target.GetHashCode();
      var entry = new Entry(hashCode, item);

      this.Entries.Add(item.Key, entry);

      if (this.TargetTable.TryGetValue(hashCode, out List<Entry> bucket))
      {
        bucket.Add(entry);
      }
      else
      {
        this.TargetTable.Add(hashCode, new List<Entry>() { entry });
      }
      if (this.IsAutoPurgeFinalizedItemsEnabled)
      {
        PurgeFinalizedItems();
      }
    }

    /// <inheritdoc />
    public void Clear()
    {
      if (this.IsReadOnly)
      {
        throw new NotSupportedException("Trying to modify a read-only collection.");
      }
      this.TargetTable.Clear();
      this.Entries.Clear();
    }

    /// <inheritdoc />
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      for (var index = arrayIndex; index < this.Entries.Count; index++)
      {
        Entry entry = this.Entries.ElementAt(index).Value;
        array[index] = entry.KeyValuePair;
      }
      if (this.IsAutoPurgeFinalizedItemsEnabled)
      {
        PurgeFinalizedItems();
      }
    }

    /// <inheritdoc />
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    /// <inheritdoc />
    public int Count => this.Entries.Count;

    /// <inheritdoc />
    public bool IsReadOnly { get; }

    #endregion

    #region Implementation of IDictionary<TKey,TValue>

    /// <inheritdoc />
    public bool ContainsKey(TKey key)
    {
      object keyValue = key.Target;
      int hashCode = keyValue.GetHashCode();
      if (this.IsAutoPurgeFinalizedItemsEnabled)
      {
        PurgeFinalizedItems();
      }
      return this.Entries.ContainsKey(key) 
             || this.TargetTable.TryGetValue(hashCode, out List<Entry> bucket)
               && bucket.Any(entry => entry.Key.Target == keyValue);
    }

    /// <inheritdoc />
    public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
      if (this.IsReadOnly)
      {
        throw new NotSupportedException("Trying to modify a read-only collection.");
      }

      return RemoveTarget(key.Target);
    }

    public bool RemoveTarget(object target)
    {
      if (this.IsReadOnly)
      {
        throw new NotSupportedException("Trying to modify a read-only collection.");
      }
      if (target == null)
      {
        if (this.IsAutoPurgeFinalizedItemsEnabled)
        {
          PurgeFinalizedItems();
        }
        return false;
      }

      int hashCode = target.GetHashCode();
      bool hasRemovedItem = false;
      if (this.TargetTable.TryGetValue(hashCode, out List<Entry> bucket))
      {
        if (bucket.Count == 1)
        {
          Entry entry = bucket.First();
          hasRemovedItem = this.TargetTable.Remove(hashCode) || this.Entries.Remove(entry.Key);
        }
        else
        {
          for (var index = bucket.Count - 1; index >= 0; index--)
          {
            Entry entry = bucket[index];
            if (entry.Key.Target.Equals(target))
            {
              bucket.RemoveAt(index);
              this.Entries.Remove(entry.Key);
              hasRemovedItem = true;
            }
          }
        }
      }

      if (this.IsAutoPurgeFinalizedItemsEnabled)
      {
        PurgeFinalizedItems();
      }
      return hasRemovedItem;
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value)
    {
      if (this.Entries.TryGetValue(key, out Entry entry))
      {
        value = entry.Value;
        return true;
      }

      return TryGetValue(key.Target, out value);
    }

    public bool TryGetValue(object target, out TValue value)
    {
      value = default;
      if (target == null)
      {
        if (this.IsAutoPurgeFinalizedItemsEnabled)
        {
          PurgeFinalizedItems();
        }
        return false;
      }

      int hashCode = target.GetHashCode();
      if (this.TargetTable.TryGetValue(hashCode, out List<Entry> bucket))
      {
        value = bucket.FirstOrDefault(entry => entry.Key.Target.Equals(target)).Value;
        return true;
      }
      return false;
    }

    /// <inheritdoc />
    public TValue this[TKey key]
    {
      get
      {
        if (TryGetValue(key, out TValue value))
        {
          return value;
        }
        throw new KeyNotFoundException();
      }
      set
      {
        if (this.IsReadOnly)
        {
          throw new NotSupportedException("Trying to modify a read-only collection.");
        }

        Add(key, value);
      }
    }

    /// <inheritdoc />
    public ICollection<TKey> Keys => this.Entries.Keys;

    /// <inheritdoc />
    public ICollection<TValue> Values => new ReadOnlyCollection<TValue>(
      this.Entries.Values.Select(entry => entry.Value).ToList());

    #endregion

    private bool TryRemoveAllFinalizedItems()
    {
      int removedCount = 0;
      for (var index = this.TargetTable.Count - 1; index >= 0; index--)
      {
        KeyValuePair<int, List<Entry>> keyValuePair = this.TargetTable.ElementAt(index);
        List<Entry> bucket = keyValuePair.Value;
        for (var bucketIndex = bucket.Count - 1; bucketIndex >= 0; bucketIndex--)
        {
          Entry entry = bucket[bucketIndex];
          if (entry.Key.Target == null)
          {
            bucket.RemoveAt(bucketIndex);
            this.Entries.Remove(entry.Key);
            removedCount++;
          }
        }
        if (!bucket.Any())
        {
          this.TargetTable.Remove(keyValuePair.Key);
        }
      }

      return removedCount > 0;
    }

    private void PurgeFinalizedItems()
    {
      Task.Run(TryRemoveAllFinalizedItems);
    }
  }
}
