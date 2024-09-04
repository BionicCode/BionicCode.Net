namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.ObjectModel;
  using System.Collections.Specialized;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Linq;

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
      CheckReentrancy();

      TItem item = this.Items[index];
      base.RemoveItem(index);

      if (item is INotifyPropertyChanged propertyChangedItem)
      {
        StopListenToItemPropertyChanged(propertyChangedItem);
      }
    }

    /// <inheritdoc />
    protected override void ClearItems()
    {
      CheckReentrancy();

      foreach (INotifyPropertyChanged item in this.Items.OfType<INotifyPropertyChanged>())
      {
        StopListenToItemPropertyChanged(item);
      }

      base.ClearItems();
    }

    /// <inheritdoc />
    protected override void SetItem(int index, TItem item)
    {
      CheckReentrancy();

      if (this.Items[index] is INotifyPropertyChanged oldPropertyChangedItem)
      {
        StopListenToItemPropertyChanged(oldPropertyChangedItem);
      }

      if (item is INotifyPropertyChanged newPropertyChangedItem)
      {
        StartListenToItemPropertyChanged(newPropertyChangedItem);
      }

      base.SetItem(index, item);
    }

    private void StartListenToItemPropertyChanged(INotifyPropertyChanged propertyChangedItem) =>

#if NET461_OR_GREATER || NET
      PropertyChangedEventManager.AddHandler(propertyChangedItem, OnItemPropertyChanged, string.Empty);
#else
      // TODO::Use custom WeakEventManager for .NET Standard support
      WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddEventHandler(propertyChangedItem, nameof(INotifyPropertyChanged.PropertyChanged), OnItemPropertyChanged);
#endif

    private void StopListenToItemPropertyChanged(INotifyPropertyChanged propertyChangedItem) =>
#if NET461_OR_GREATER || NET
      PropertyChangedEventManager.RemoveHandler(propertyChangedItem, OnItemPropertyChanged, string.Empty);
#else
      // TODO::Use custom WeakEventManager for .NET Standard support
      WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.RemoveEventHandler(propertyChangedItem, nameof(INotifyPropertyChanged.PropertyChanged), OnItemPropertyChanged);
#endif

    #endregion Overrides of ObservableCollection<TItem>

    public PropertyChangedEventHandler ItemPropertyChanged;

    private void OnItemPropertyChanged(object item, PropertyChangedEventArgs e)
      => this.ItemPropertyChanged?.Invoke(item, e);
  }  
}
