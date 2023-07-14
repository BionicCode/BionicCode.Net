namespace BionicCode.Utilities.Net
{
  #region Info

  // 2020/11/17  14:50
  // Net.Wpf

  #endregion

  #region Usings

  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Collections.Specialized;
  #endregion

  /// <summary>
  /// A <see cref="Queue{T}"/> that implements <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>.
  /// <br/> Since the behavior itself is not changed, see <see cref="Queue{T}"/> for a detailed API documentation.
  /// </summary>
  /// <typeparam name="TItem"></typeparam>
  public class ObservableQueue<TItem> : Queue<TItem>, INotifyCollectionChanged, INotifyPropertyChanged
  {
    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;
    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <summary>
    /// Adds an object to the end of the <see cref="Queue{T}"/>.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="Queue{T}"/>. The value can be <c>null</c> for reference types.</param>
    new public void Enqueue(TItem item)
    {
      base.Enqueue(item);
      OnPropertyChanged();
      OnCollectionChanged(NotifyCollectionChangedAction.Add, item, this.Count - 1);
    }

    /// <summary>
    /// Removes and returns the object at the beginning of the <see cref="Queue{T}"/>.
    /// </summary>
    /// <returns></returns>
    new public TItem Dequeue()
    {
      TItem removedItem = base.Dequeue();
      OnPropertyChanged();
      OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, 0);
      return removedItem;
    }

#if NET
    /// <summary>
    /// Removes the object at the beginning of the <see cref="Queue{T}"/>, and copies it to the result parameter.
    /// </summary>
    /// <param name="result">The removed object.</param>
    /// <returns><c>true</c> if the object is successfully removed; <c>false</c> if the <see cref="Queue{T}"/> is empty.</returns>
    new public bool TryDequeue(out TItem result)
    {
      if (base.TryDequeue(out result))
      {
        OnPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, result, 0);
        return true;
      }

      return false;
    }
#endif

    /// <summary>
    /// Removes all objects from the <see cref="Queue{T}"/>.
    /// </summary>
    new public void Clear()
    {
      base.Clear();
      OnPropertyChanged();
      OnCollectionChangedReset();
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, TItem item, int index)
      => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item, index));

    private void OnCollectionChangedReset()
      => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

    private void OnPropertyChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Count)));
  }
}