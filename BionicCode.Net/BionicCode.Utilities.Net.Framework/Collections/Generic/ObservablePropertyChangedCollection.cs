using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace BionicCode.Utilities.Net.Framework.Collections.Generic
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
        PropertyChangedEventManager.AddHandler(propertyChangedItem, OnItemPropertyChanged, string.Empty);
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
          PropertyChangedEventManager.RemoveHandler(propertyChangedItem, OnItemPropertyChanged, string.Empty);
        }
      }

      base.RemoveItem(index);
    }

    /// <inheritdoc />
    protected override void ClearItems()
    {
      this.Items.OfType<INotifyPropertyChanged>()
        .ToList()
        .ForEach(propertyChangedItem =>
          PropertyChangedEventManager.RemoveHandler(propertyChangedItem, OnItemPropertyChanged, string.Empty));

      base.ClearItems();
    }

    /// <inheritdoc />
    protected override void SetItem(int index, TItem item)
    {
      if (index < this.Count)
      {
        if (this.Items[index] is INotifyPropertyChanged oldPropertyChangedItem)
        {
          PropertyChangedEventManager.RemoveHandler(oldPropertyChangedItem, OnItemPropertyChanged, string.Empty);
        }

        if (item is INotifyPropertyChanged newPropertyChangedItem)
        {
          PropertyChangedEventManager.AddHandler(newPropertyChangedItem, OnItemPropertyChanged, string.Empty);
        }
      }

      base.SetItem(index, item);
    }

    #endregion Overrides of ObservableCollection<TItem>

    private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, sender, IndexOf((TItem)sender)));
    }
  }
}
