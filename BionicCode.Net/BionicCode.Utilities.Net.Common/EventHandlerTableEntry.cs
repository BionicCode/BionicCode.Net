namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Collections;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Xml.Linq;

  internal readonly struct EventHandlerTableEntry : IEquatable<EventHandlerTableEntry>
  {
    public EventHandlerTableEntry(Delegate generatedHandler, EventInfo sourceEventInfo, object eventSource)
    {
      this.GeneratedHandler = generatedHandler;
      this.SourceEventInfo = sourceEventInfo;
      this.EventSourceInstances = new WeakReference<object>(eventSource);
    }

    public Delegate GeneratedHandler { get; }
    public EventInfo SourceEventInfo { get; }
    public IList<WeakReference<object>> EventSourceInstances { get; }

    public bool Equals(EventHandlerTableEntry other) => ReferenceEquals(other.GeneratedHandler, this.GeneratedHandler) && other.SourceEventInfo.Equals(this.SourceEventInfo) && (other.EventSourceInstances.TryGetTarget(out object otherEventSource) && this.EventSourceInstances.TryGetTarget(out object eventSource) ? ReferenceEquals(otherEventSource, eventSource) : true);
    public override bool Equals(object obj) => obj is EventHandlerTableEntry entry && Equals(entry);

    public override int GetHashCode()
    {
      int hashCode = 760839574;
      hashCode = hashCode * -1521134295 + EqualityComparer<Delegate>.Default.GetHashCode(this.GeneratedHandler);
      hashCode = hashCode * -1521134295 + EqualityComparer<EventInfo>.Default.GetHashCode(this.SourceEventInfo);
      hashCode = hashCode * -1521134295 + EqualityComparer<WeakReference<object>>.Default.GetHashCode(this.EventSourceInstances);
      return hashCode;
    }

    public static bool operator ==(EventHandlerTableEntry left, EventHandlerTableEntry right) => left.Equals(right);
    public static bool operator !=(EventHandlerTableEntry left, EventHandlerTableEntry right) => !(left == right);
  }

  internal static class WeakReferencePool
  {
    private static Queue<WeakReference<object>> WeakReferences { get; } = new Queue<WeakReference<object>>();
    public static void Add(WeakReference<object> weakReference)
    {
      weakReference.SetTarget(null);
      WeakReferences.Enqueue(weakReference);
    }

    public static WeakReference<object> GetOrCreate(object reference)
    {
      WeakReference<object> weakReference;
      if (WeakReferences.Count > 0)
      {
        weakReference = WeakReferences.Dequeue();
        weakReference.SetTarget(reference);
      }
      else
      {
        weakReference = new WeakReference<object>(reference);
      }

      return weakReference;
    }
  }

  internal class WeakReferenceCollection<TItem> : ICollection<TItem>, INotifyPropertyChanged where TItem : class
  {
    protected IList<WeakReference<object>> Items { get; }

    public TItem this[int index]
    {
      get
      {
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(index, this.Count, nameof(index));

        WeakReference<object> reference = this.Items[index];
        bool isAlive = reference.TryGetTarget(out object target);
        if (isAlive)
        {
          return (TItem)target;
        }
        else 
        {
          RemoveItem(index);
          OnCountChanged();
          OnIndexerChanged();

          return null;
        }
      }

      set
      {
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(index, this.Count, nameof(index));
        if (this.IsReadOnly)
        {
          throw new NotSupportedException("Collection is read-only");
        }

        SetItem(index, value);
      }
    }

    public int Count
    {
      get
      {
        PurgeDeadReferences();
        return this.Items.Count;
      }
    }

    public bool IsReadOnly { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    #region Constructors

    public WeakReferenceCollection()
    {
      this.Items = new List<WeakReference<object>>();
      this.IsReadOnly = false;
    }

    public WeakReferenceCollection(IList<WeakReference<object>> list)
    {
      this.Items = list.Select(WeakReferencePool.GetOrCreate).ToList();
      this.IsReadOnly = list.IsReadOnly;
    }

    public WeakReferenceCollection(IEnumerable<WeakReference<object>> items)
    {
      this.Items = items.Select(WeakReferencePool.GetOrCreate).ToList();
      this.IsReadOnly = false;
    }

    public WeakReferenceCollection(bool isReadOnly)
    {
      this.Items = new List<WeakReference<object>>();
      this.IsReadOnly = isReadOnly;
    }

    public WeakReferenceCollection(IList<WeakReference<object>> list, bool isReadOnly)
    {
      this.Items = list.Select(WeakReferencePool.GetOrCreate).ToList();
      this.IsReadOnly = list.IsReadOnly || isReadOnly;
    }

    public WeakReferenceCollection(IEnumerable<TItem> items, bool isReadOnly)
    {
      this.Items = items.Select(WeakReferencePool.GetOrCreate).ToList();
      this.IsReadOnly = isReadOnly;
    }

    #endregion Constructors

    public bool TryGet(int index, out TItem item)
    {
      item = this[index];
      return item != null;
    }

    public void Add(TItem item)
    {
      if (this.IsReadOnly)
      {
        throw new NotSupportedException("Collection is read-only");
      }

      int index = this.Items.Count;
      InsertItem(index, item);
      OnCountChanged();
      OnIndexerChanged();
    }

    public void Clear()
    {
      if (this.IsReadOnly)
      {
        throw new NotSupportedException("Collection is read-only");
      }

      bool hasChanges = this.Items.Any();

      ClearItems();
      if (hasChanges)
      {
        OnCountChanged();
        OnIndexerChanged();
      }
    }

    public bool Contains(TItem item)
    {
      bool hasCountChanged = false;
      for (int index = this.Items.Count - 1; index >= 0; index--)
      {
        WeakReference<object> reference = this.Items[index];
        bool isAlive = reference.TryGetTarget(out object target);
        if (isAlive && ReferenceEquals(item, target))
        {
          return true;
        }
        else if (!isAlive)
        {
          RemoveItem(index);
          hasCountChanged = true;
        }
      }

      if (hasCountChanged)
      {
        OnCountChanged();
        OnIndexerChanged();
      }

      return false;
    }

    public void CopyTo(TItem[] array, int arrayIndex)
    {
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(arrayIndex, nameof(arrayIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length, nameof(arrayIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfLessThan(array.Length - arrayIndex, this.Items.Count , nameof(arrayIndex));

      for (int index = 0; index < this.Items.Count; index++)
      {
        WeakReference<object> reference = this.Items[index];
        bool isAlive = reference.TryGetTarget(out object target);
        if (isAlive)
        {
          array[arrayIndex++] = (TItem)target;
        }
      }
    }

    public IEnumerator<TItem> GetEnumerator()
    {
      WeakReference<object>[] items = this.Items.ToArray();
      bool hasCountChanged = false;
      for (int index = 0; index < this.Items.Count; index++)
      {
        WeakReference<object> reference = items[index];
        if (reference.TryGetTarget(out object target))
        {
          yield return (TItem)target;
        }
        else
        {
          RemoveItem(index);
          hasCountChanged = true;
        }
      }

      if (hasCountChanged)
      {
        OnCountChanged();
        OnIndexerChanged();
      }
    }

    public bool Remove(TItem item)
    {
      if (this.IsReadOnly)
      {
        throw new NotSupportedException("Collection is read-only");
      }

      bool hasCountChanged = false;
      for (int index = this.Items.Count - 1; index >= 0; index--)
      {
        WeakReference<object> reference = this.Items[index];
        bool isAlive = reference.TryGetTarget(out object target);
        if (isAlive && ReferenceEquals(item, target))
        {
          RemoveItem(index);
          OnCountChanged();
          OnIndexerChanged();

          return true;
        }
        else if (!isAlive)
        {
          RemoveItem(index);
          hasCountChanged = true;
        }
      }

      if (hasCountChanged)
      {
        OnCountChanged();
        OnIndexerChanged();
      }

      return false;
    }

    protected virtual void ClearItems()
    {
      for (int index = this.Items.Count - 1; index >= 0; index--)
      {
        RemoveItem(index);
      }
    }

    protected virtual void InsertItem(int index, TItem item)
    {
      WeakReference<object> reference = WeakReferencePool.GetOrCreate(item);
      this.Items.Insert(index, reference);
    }

    protected virtual void RemoveItem(int index)
    {
      WeakReference<object> reference = this.Items[index];
      WeakReferencePool.Add(reference);
      this.Items.RemoveAt(index);
    }

    protected virtual void SetItem(int index, TItem item) => this.Items[index].SetTarget(item);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected virtual void OnCountChanged()
      => this.PropertyChanged?.Invoke(this, Common.CountPropertyChangedEventArgs);

    protected virtual void OnIndexerChanged()
      => this.PropertyChanged?.Invoke(this, Common.IndexerPropertyChangedEventArgs);

    private void PurgeDeadReferences()
    {
      bool hasCountChanged = false;
      for (int index = this.Items.Count - 1; index >= 0; index--)
      {
        WeakReference<object> reference = this.Items[index];
        bool isAlive = reference.TryGetTarget(out _);
        if (!isAlive)
        {
          RemoveItem(index);
          hasCountChanged = true;
        }
      }

      if (hasCountChanged)
      {
        OnCountChanged();
        OnIndexerChanged();
      }
    }
  }
}