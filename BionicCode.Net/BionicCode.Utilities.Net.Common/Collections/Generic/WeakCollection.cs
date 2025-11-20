namespace BionicCode.Utilities.Net
{
    #region Info
    // //  
    // BionicUtilities.Net.Standard
    #endregion

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Collection that internally wraps its items into <see cref="WeakReference{TItem}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <remarks>
    /// <para>
    /// The collection internally maintains its weak references in that it removes those that have been garbage collected.
    /// As a result the collection's count is not constant and can change any time when a weak reference is no longer alive (which is when the target of that reference has been garbage collected).
    /// </para>
    /// <para>
    /// Calling <see cref="WeakCollection{TItem}.Count"/> is an O(n) operation as it will remove all dead weak references in <see langword="orderby"/>to return the correct count.
    /// </para>
    /// <para>
    /// This collection implements <see cref="INotifyPropertyChanged"/> to enable clients to listen to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event to get notified when the <see cref="WeakCollection{TItem}.Count"/> property has changed.
    /// </para>
    /// <para>
    /// Call <see cref="WeakCollection{TItem}.IsAlive(int)"/> to know if an item is still alive.
    /// </para>
    /// <para>
    /// Call <see cref="WeakCollection{TItem}.TryGet(int, out TItem)"/> to conveniently retrieve an item if it is still alive.
    /// </para>
    /// </remarks>
    public class WeakCollection<TItem> : ICollection<TItem>, INotifyPropertyChanged where TItem : class
    {
        protected IList<WeakReference<object>> Items { get; }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <value>The element at the specified index if the object is still alive. Otherwise <see langword="null"/>.</value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>The <paramref name="index"/> is greater than or equal to the count of the current <see cref="WeakCollection{TItem}"/>.</item>
        /// <item>The <paramref name="index"/> is negative.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException">The collection is read-only and <see cref="IsReadOnly"/> returns <see langword="true"/>.</exception>
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

        /// <summary>
        /// Returns the item count of this collection.
        /// </summary>
        /// <remarks>
        /// Accessing the <see cref="WeakCollection{TItem}.Count"/> property is an O(n) operation as it will remove all dead weak references in order to return the correct count.
        /// </remarks>
        /// <value>The updated number of items that are alive and contained in this collection.</value>
        public int Count
        {
            get
            {
                PurgeDeadReferences();
                return this.Items.Count;
            }
        }

        /// <summary>
        /// Indicates whether the collection is read-only or not.
        /// </summary>
        /// <remarks>
        /// Read-only state can only be changed by calling the appropriate constructor that either takes a <see langword="bool"/> or <see langword="abstract"/> collection where <see cref="ICollection{T}.IsReadOnly"/> returns <see langword="true"/>.
        /// </remarks>
        /// <value>Returns <see langword="true"/> if this collection can't be modified. Otherwise <see langword="false"/>.</value>
        public bool IsReadOnly { get; }

        /// <inherit>>
        public event PropertyChangedEventHandler PropertyChanged;

        #region Constructors

        public WeakCollection()
        {
            this.Items = new List<WeakReference<object>>();
            this.IsReadOnly = false;
        }

        public WeakCollection(ICollection<WeakReference<object>> collection)
        {
            ArgumentNullExceptionEx.ThrowIfNull(collection, nameof(collection));

            this.Items = collection.Select(WeakReferencePool.GetOrCreate).ToList();
            this.IsReadOnly = collection.IsReadOnly;
        }

        public WeakCollection(IEnumerable<WeakReference<object>> items)
        {
            ArgumentNullExceptionEx.ThrowIfNull(items, nameof(items));

            this.Items = items.Select(WeakReferencePool.GetOrCreate).ToList();
            this.IsReadOnly = false;
        }

        public WeakCollection(bool isReadOnly)
        {
            this.Items = new List<WeakReference<object>>();
            this.IsReadOnly = isReadOnly;
        }

        public WeakCollection(ICollection<WeakReference<object>> collection, bool isReadOnly)
        {
            ArgumentNullExceptionEx.ThrowIfNull(collection, nameof(collection));

            this.Items = collection.Select(WeakReferencePool.GetOrCreate).ToList();
            this.IsReadOnly = collection.IsReadOnly || isReadOnly;
        }

        public WeakCollection(IEnumerable<TItem> items, bool isReadOnly)
        {
            ArgumentNullExceptionEx.ThrowIfNull(items, nameof(items));

            this.Items = items.Select(WeakReferencePool.GetOrCreate).ToList();
            this.IsReadOnly = isReadOnly;
        }

        #endregion Constructors

        /// <summary>
        /// Attempts to get the object at the specified index, if the object is still alive.
        /// </summary>
        /// <param name="index">The zero-based index of the item to return.</param>
        /// <param name="item">The object <see langword="if"/>alive; otherwise <see langword="null"/></param>
        /// <returns><see langword="true"/> if the object is alive. Otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>The <paramref name="index"/> is greater than or equal the count of the current <see cref="WeakCollection{TItem}"/>.</item>
        /// <item>The <paramref name="index"/> is negative.</item>
        /// </list></exception>
        public bool TryGet(int index, out TItem item)
        {
            item = this[index];
            return item != null;
        }

        /// <summary>
        /// Checks whether the item at a particular index is still alive.
        /// </summary>
        /// <param name="index">The zero-based index of the item to check.</param>
        /// <returns>Returns <see langword="true"/> if the item is still alive and therefore not yet garbage collected. Otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>The <paramref name="index"/> is greater than or equal the count of the current <see cref="WeakCollection{TItem}"/>.</item>
        /// <item>The <paramref name="index"/> is negative.</item>
        /// </list></exception>
        public bool IsAlive(int index)
        {
            TItem item = this[index];
            return item != null;
        }

        /// <summary>
        /// Adds a new item. The item is stored as a <see cref="WeakReference"/> internally to allow it to get garbage collected. Duplicate items are allowed.
        /// </summary>
        /// <param name="item">The item to add to the collection.</param>
        /// <exception cref="NotSupportedException">The collection is read-only and <see cref="IsReadOnly"/> returns <see langword="true"/>.</exception>
        /// <remarks>
        /// The <see cref="WeakCollection{TItem}"/> only keeps weak references to its items. Therefore it will not keep items alive that are not referenced by a strong reference in the scope of the client.
        /// </remarks>
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

        /// <summary>
        /// Clears the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">The collection is read-only and <see cref="IsReadOnly"/> returns <see langword="true"/>.</exception>
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

        /// <summary>
        /// Determines whether the <see cref="WeakCollection{TItem}"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="WeakCollection{TItem}"/>. The value can be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="item"/> is found and alive in the <see cref="WeakCollection{TItem}"/>. Otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Copies the elements of the <see cref="WeakCollection{TItem}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="WeakCollection{TItem}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>The <paramref name="arrayIndex"/> is greater than or equal the length of <paramref name="array"/>.</item>
        /// <item>The <paramref name="arrayIndex"/> results in insufficient length of <paramref name="array"/>.</item>
        /// <item>The <paramref name="arrayIndex"/> is negative.</item>
        /// </list></exception>
        /// <exception cref="ArgumentException"><paramref name="array"/> is too small.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is <see langword="null"/>.</exception>
        public void CopyTo(TItem[] array, int arrayIndex)
        {
            ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
            ArgumentOutOfRangeExceptionEx.ThrowIfNegative(arrayIndex, nameof(arrayIndex));
            ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length, nameof(arrayIndex));
            int availableArrayLength = array.Length - arrayIndex;
            if (availableArrayLength < this.Items.Count)
            {
                throw new ArgumentException("The array is too small", nameof(array));
            }

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

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
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

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="WeakCollection{TItem}"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="WeakCollection{TItem}"/>.</param>
        /// <returns><see langword="true"/> if item was successfully removed from the <see cref="WeakCollection{TItem}"/>; otherwise, <see langword="false"/>. 
        /// This method also returns <see langword="false"/> if item is not found in the original <see cref="WeakCollection{TItem}"/>.</returns>
        /// <exception cref="NotSupportedException">The collection is read-only and <see cref="IsReadOnly"/> returns <see langword="true"/>.</exception>
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
            this.Items.RemoveAt(index);
            WeakReferencePool.Add(reference);
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
