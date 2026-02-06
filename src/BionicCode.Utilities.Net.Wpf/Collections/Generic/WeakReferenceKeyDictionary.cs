//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Threading.Tasks;

//namespace BionicCode.Utilities.Net
//{
//  public class WeakReferenceKeyDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : WeakReference
//  {
//    internal class Entry
//    {
//      public Entry(int hashCode, TKey key, TValue value) : this(hashCode, new KeyValuePair<TKey, TValue>(key, value))
//      {
//      }
//      public Entry(int hashCode, KeyValuePair<TKey, TValue> keyValuePair)
//      {
//        Id = Guid.NewGuid();
//        HashCode = hashCode;
//        Key = keyValuePair.Key;
//        ExecuteDelegate = keyValuePair.ExecuteDelegate;
//        KeyValuePair = keyValuePair;
//      }

//      public int HashCode { get; set; }
//      public TKey Key { get; set; }
//      public TValue ExecuteDelegate { get; set; }
//      public KeyValuePair<TKey, TValue> KeyValuePair { get; set; }
//      public Guid Id { get; set; }
//    }

//    public WeakReferenceKeyDictionary() : this(false, false)
//    { }

//    public WeakReferenceKeyDictionary(bool isReadOnly, bool isAutoPurgeFinalizedItemsEnabled)
//    {
//      IsReadOnly = isReadOnly;
//      IsAutoPurgeFinalizedItemsEnabled = isAutoPurgeFinalizedItemsEnabled;
//      TargetTable = new Dictionary<int, List<Entry>>();
//      Entries = new Dictionary<TKey, Entry>();
//    }

//    public bool IsAutoPurgeFinalizedItemsEnabled { get; set; }
//    private Dictionary<int, List<Entry>> TargetTable { get; }
//    private Dictionary<TKey, Entry> Entries { get; }

//    #region Implementation of IEnumerable

//    /// <inheritdoc />
//    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
//    {
//      foreach (KeyValuePair<TKey, Entry> keyValuePair in Entries)
//      {
//        yield return keyValuePair.ExecuteDelegate.KeyValuePair;
//      }
//    }

//    /// <inheritdoc />
//    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

//    #endregion

//    #region Implementation of ICollection<KeyValuePair<TKey,TValue>>

//    /// <inheritdoc />
//    public void Add(KeyValuePair<TKey, TValue> item)
//    {
//      if (IsReadOnly)
//      {
//        throw new NotSupportedException("Trying to modify a read-only collection.");
//      }

//      object target = item.Key.Target;
//      if (target == null)
//      {
//        return;
//      }
//      int hashCode = target.GetHashCode();
//      var entry = new Entry(hashCode, item);

//      Entries.Add(item.Key, entry);

//      if (TargetTable.TryGetValue(hashCode, out List<Entry> bucket))
//      {
//        bucket.Add(entry);
//      }
//      else
//      {
//        TargetTable.Add(hashCode, new List<Entry>() { entry });
//      }
//      if (IsAutoPurgeFinalizedItemsEnabled)
//      {
//        PurgeFinalizedItems();
//      }
//    }

//    /// <inheritdoc />
//    public void Clear()
//    {
//      if (IsReadOnly)
//      {
//        throw new NotSupportedException("Trying to modify a read-only collection.");
//      }
//      TargetTable.Clear();
//      Entries.Clear();
//    }

//    /// <inheritdoc />
//    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);

//    /// <inheritdoc />
//    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
//    {
//      for (var index = arrayIndex; index < Entries.Count; index++)
//      {
//        Entry entry = Entries.ElementAt(index).ExecuteDelegate;
//        array[index] = entry.KeyValuePair;
//      }
//      if (IsAutoPurgeFinalizedItemsEnabled)
//      {
//        PurgeFinalizedItems();
//      }
//    }

//    /// <inheritdoc />
//    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

//    /// <inheritdoc />
//    public int Count => Entries.Count;

//    /// <inheritdoc />
//    public bool IsReadOnly { get; }

//    #endregion

//    #region Implementation of IDictionary<TKey,TValue>

//    /// <inheritdoc />
//    public bool ContainsKey(TKey key)
//    {
//      object keyValue = key.Target;
//      int hashCode = keyValue.GetHashCode();
//      if (IsAutoPurgeFinalizedItemsEnabled)
//      {
//        PurgeFinalizedItems();
//      }
//      return Entries.ContainsKey(key) 
//             || TargetTable.TryGetValue(hashCode, out List<Entry> bucket)
//               && bucket.Any(entry => entry.Key.Target == keyValue);
//    }

//    /// <inheritdoc />
//    public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));

//    /// <inheritdoc />
//    public bool Remove(TKey key)
//    {
//      if (IsReadOnly)
//      {
//        throw new NotSupportedException("Trying to modify a read-only collection.");
//      }

//      return RemoveTarget(key.Target);
//    }

//    public bool RemoveTarget(object target)
//    {
//      if (IsReadOnly)
//      {
//        throw new NotSupportedException("Trying to modify a read-only collection.");
//      }
//      if (target == null)
//      {
//        if (IsAutoPurgeFinalizedItemsEnabled)
//        {
//          PurgeFinalizedItems();
//        }
//        return false;
//      }

//      int hashCode = target.GetHashCode();
//      bool hasRemovedItem = false;
//      if (TargetTable.TryGetValue(hashCode, out List<Entry> bucket))
//      {
//        if (bucket.Count == 1)
//        {
//          Entry entry = bucket.First();
//          hasRemovedItem = TargetTable.Remove(hashCode) || Entries.Remove(entry.Key);
//        }
//        else
//        {
//          for (var index = bucket.Count - 1; index >= 0; index--)
//          {
//            Entry entry = bucket[index];
//            if (entry.Key.Target.Equals(target))
//            {
//              bucket.RemoveAt(index);
//              Entries.Remove(entry.Key);
//              hasRemovedItem = true;
//            }
//          }
//        }
//      }

//      if (IsAutoPurgeFinalizedItemsEnabled)
//      {
//        PurgeFinalizedItems();
//      }
//      return hasRemovedItem;
//    }

//    /// <inheritdoc />
//    public bool TryGetValue(TKey key, out TValue value)
//    {
//      if (Entries.TryGetValue(key, out Entry entry))
//      {
//        value = entry.ExecuteDelegate;
//        return true;
//      }

//      return TryGetValue(key.Target, out value);
//    }

//    public bool TryGetValue(object target, out TValue value)
//    {
//      value = default;
//      if (target == null)
//      {
//        if (IsAutoPurgeFinalizedItemsEnabled)
//        {
//          PurgeFinalizedItems();
//        }
//        return false;
//      }

//      int hashCode = target.GetHashCode();
//      if (TargetTable.TryGetValue(hashCode, out List<Entry> bucket))
//      {
//        value = bucket.FirstOrDefault(entry => entry.Key.Target.Equals(target)).ExecuteDelegate;
//        return true;
//      }
//      return false;
//    }

//    /// <inheritdoc />
//    public TValue this[TKey key]
//    {
//      get
//      {
//        if (TryGetValue(key, out TValue value))
//        {
//          return value;
//        }
//        throw new KeyNotFoundException();
//      }
//      set
//      {
//        if (IsReadOnly)
//        {
//          throw new NotSupportedException("Trying to modify a read-only collection.");
//        }

//        Add(key, value);
//      }
//    }

//    /// <inheritdoc />
//    public ICollection<TKey> Keys => Entries.Keys;

//    /// <inheritdoc />
//    public ICollection<TValue> Values => new ReadOnlyCollection<TValue>(
//      Entries.Values.Select(entry => entry.ExecuteDelegate).ToList());

//    #endregion

//    private bool TryRemoveAllFinalizedItems()
//    {
//      int removedCount = 0;
//      for (var index = TargetTable.Count - 1; index >= 0; index--)
//      {
//        KeyValuePair<int, List<Entry>> keyValuePair = TargetTable.ElementAt(index);
//        List<Entry> bucket = keyValuePair.ExecuteDelegate;
//        for (var bucketIndex = bucket.Count - 1; bucketIndex >= 0; bucketIndex--)
//        {
//          Entry entry = bucket[bucketIndex];
//          if (entry.Key.Target == null)
//          {
//            bucket.RemoveAt(bucketIndex);
//            Entries.Remove(entry.Key);
//            removedCount++;
//          }
//        }
//        if (!bucket.Any())
//        {
//          TargetTable.Remove(keyValuePair.Key);
//        }
//      }

//      return removedCount > 0;
//    }

//    private void PurgeFinalizedItems()
//    {
//      Task.Run(TryRemoveAllFinalizedItems);
//    }
//  }
//}
