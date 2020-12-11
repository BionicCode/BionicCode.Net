using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace BionicCode.Utilities.Net.Core.Wpf.Collections.Generic
{
  /// <summary>
  /// Raises <see cref="ObservableCollection{T}.CollectionChanged"></see> event when the property of an item raised <see cref="INotifyPropertyChanged.PropertyChanged"/>. The <see cref="NotifyCollectionChangedAction"/> for this particular notification is <see cref="NotifyCollectionChangedAction.Reset"/> with a reference to the notifying item and the item's index. .
  /// </summary>
  /// <typeparam name="TItem"></typeparam>
  /// <remarks>The item must implement <see cref="INotifyPropertyChanged"/> otherwise the behavior is like a common <see cref="ObservableCollection{T}"/>. The <see cref="ObservablePropertyChangedCollection{TItem}"/> implements the weak event pattern in order to handle the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.</remarks>
  [DebuggerDisplay("Count = {Count}")]
  [Serializable]
  public class ObservablePropertyChangedCollection<TItem> : ObservableCollection<TItem>
  {
    #region Overrides of ObservableCollection<TItem>

    /// <inheritdoc />
    protected override void InsertItem(int index, TItem item)
    {
      base.InsertItem(index, item);
      if (item is INotifyPropertyChanged propertyChangedItem)
      {
        StartListenToItemPropertyChanged(propertyChangedItem);
      }
    }

    /// <inheritdoc />
    protected override void RemoveItem(int index)
    {
      if (index < this.Count)
      {
        TItem item = this.Items[index];
        if (item is INotifyPropertyChanged propertyChangedItem)
        {
          StopListenToItemPropertyChanged(propertyChangedItem);
        }
      }

      base.RemoveItem(index);
    }

    /// <inheritdoc />
    protected override void ClearItems()
    {
      this.Items.OfType<INotifyPropertyChanged>()
        .ToList()
        .ForEach(StopListenToItemPropertyChanged);

      base.ClearItems();
    }

    /// <inheritdoc />
    protected override void SetItem(int index, TItem item)
    {
      if (index < this.Count)
      {
        if (this.Items[index] is INotifyPropertyChanged oldPropertyChangedItem)
        {
          StopListenToItemPropertyChanged(oldPropertyChangedItem);
        }

        if (item is INotifyPropertyChanged newPropertyChangedItem)
        {
          StartListenToItemPropertyChanged(newPropertyChangedItem);
        }
      }

      base.SetItem(index, item);
    }

    private void StartListenToItemPropertyChanged(INotifyPropertyChanged propertyChangedItem) => PropertyChangedEventManager.AddHandler(propertyChangedItem, OnItemPropertyChanged, string.Empty);

    private void StopListenToItemPropertyChanged(INotifyPropertyChanged propertyChangedItem) => PropertyChangedEventManager.RemoveHandler(propertyChangedItem, OnItemPropertyChanged, string.Empty);

    #endregion Overrides of ObservableCollection<TItem>

    private PropertyChangedEventHandler childPropertyChanged;

    #region Overrides of ObservableCollection<TItem>

    /// <inheritdoc />
    protected override event PropertyChangedEventHandler PropertyChanged
    {
      add
      {
        base.PropertyChanged += value;
        this.childPropertyChanged = (PropertyChangedEventHandler)Delegate.Combine(this.childPropertyChanged, value);
      }
      remove
      {
        base.PropertyChanged -= value;
        this.childPropertyChanged = (PropertyChangedEventHandler)Delegate.Remove(this.childPropertyChanged, value);
      }
    }

    #endregion

    private void OnItemPropertyChanged(object item, PropertyChangedEventArgs e)
    {
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, IndexOf((TItem)item), IndexOf((TItem)item)));

      this.childPropertyChanged?.Invoke(item, e);
    }
  }
}
