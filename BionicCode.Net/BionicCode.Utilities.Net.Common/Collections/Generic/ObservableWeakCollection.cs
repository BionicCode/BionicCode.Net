namespace BionicCode.Utilities.Net
{
    #region Info
    // //  
    // BionicUtilities.Net.Standard
    #endregion

    //public class ObservableWeakCollection<TItem> : WeakCollection<TItem>, INotifyCollectionChanged where TItem : class
    //{
    //  private int blockReentrancyCount;

    //  public event NotifyCollectionChangedEventHandler CollectionChanged;

    //  public ObservableWeakCollection()
    //  {
    //  }

    //  public ObservableWeakCollection(IList<WeakReference<object>> list) : base(list)
    //  {
    //  }

    //  public ObservableWeakCollection(IEnumerable<WeakReference<object>> items) : base(items)
    //  {
    //  }

    //  public ObservableWeakCollection(bool isReadOnly) : base(isReadOnly)
    //  {
    //  }

    //  public ObservableWeakCollection(IList<WeakReference<object>> list, bool isReadOnly) : base(list, isReadOnly)
    //  {
    //  }

    //  public ObservableWeakCollection(IEnumerable<TItem> items, bool isReadOnly) : base(items, isReadOnly)
    //  {
    //  }

    //  protected override void ClearItems() => base.ClearItems();
    //  protected override void InsertItem(int index, TItem item) => base.InsertItem(index, item);
    //  protected override void RemoveItem(int index) => base.RemoveItem(index);
    //  protected override void SetItem(int index, TItem item) => base.SetItem(index, item);

    //  /// <summary>
    //  /// Disallow reentrant attempts to change this collection. E.g. an event handler
    //  /// of the CollectionChanged event is not allowed to make changes to this collection.
    //  /// </summary>
    //  /// <remarks>
    //  /// typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:
    //  /// <code>
    //  ///         using (BlockReentrancy())
    //  ///         {
    //  ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
    //  ///         }
    //  /// </code>
    //  /// </remarks>
    //  protected IDisposable BlockReentrancy()
    //  {
    //    blockReentrancyCount++;
    //    return EnsureMonitorInitialized();
    //  }

    //  /// <summary> Check and assert for reentrant attempts to change this collection. </summary>
    //  /// <exception cref="InvalidOperationException"> raised when changing the collection
    //  /// while another collection change is still being notified to other listeners </exception>
    //  protected void CheckReentrancy()
    //  {
    //    if (blockReentrancyCount > 0)
    //    {
    //      // we can allow changes if there's only one listener - the problem
    //      // only arises if reentrant changes make the original event args
    //      // invalid for later listeners.  This keeps existing code working
    //      // (e.g. Selector.SelectedItems).
    //      NotifyCollectionChangedEventHandler? handler = CollectionChanged;
    //      if (handler != null && !handler.HasSingleTarget)
    //        throw new InvalidOperationException(SR.ObservableCollectionReentrancyNotAllowed);
    //    }
    //  }
    //}
}
