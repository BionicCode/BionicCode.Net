namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A thread-safe hash set implemented on top of <see cref="ConcurrentDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <remarks>
    /// <para>
    /// <b>Null handling:</b> When <typeparamref name="T"/> is a reference type, this set supports storing
    /// <see langword="null"/>. Because <see cref="ConcurrentDictionary{TKey,TValue}"/> does not support
    /// <see langword="null"/> keys, <see langword="null"/> is tracked separately.
    /// </para>
    ///
    /// <para>
    /// <b>Enumeration behavior:</b> differs by <see cref="EnumerationMode"/>:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="ConcurrentHashSetEnumerationMode.Live"/>: uses the underlying dictionary enumerator.
    /// It is safe to enumerate concurrently with reads/writes, but it is <b>not</b> a moment-in-time snapshot;
    /// enumeration may observe modifications made after enumeration starts. ([Official Docs] :contentReference[oaicite:1]{index=1})
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="ConcurrentHashSetEnumerationMode.Snapshot"/>: each enumeration enumerates a snapshot created when
    /// enumeration starts (stable/deterministic view), at the cost of allocating and copying.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="ConcurrentHashSetEnumerationMode.SnapshotOnDemand"/>: default enumeration is live (like <c>Live</c>),
    /// but <see cref="Snapshot"/> provides an explicit stable view.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// <b>Pros / cons:</b> Live enumeration avoids snapshot allocations and is typically cheapest, but can be surprising
    /// if you expect stable iteration. Snapshot enumeration provides stable results but costs O(n) memory/time per enumeration.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var set = new ConcurrentHashSet&lt;string?&gt;(ConcurrentHashSetEnumerationMode.SnapshotOnDemand);
    /// set.TryAdd("a");
    /// set.TryAdd(null);
    ///
    /// // Live enumeration (may observe concurrent mutations):
    /// foreach (var item in set)
    /// {
    ///     Console.WriteLine(item ?? "&lt;null&gt;");
    /// }
    ///
    /// // Deterministic enumeration (moment-in-time):
    /// var snapshot = set.Snapshot();
    /// foreach (var item in snapshot)
    /// {
    ///     Console.WriteLine(item ?? "&lt;null&gt;");
    /// }
    ///
    /// // Span-based read-only access (allocation-free view over the snapshot array), fastest:
    /// ReadOnlySpan&lt;string?&gt; span = snapshot.Span;
    /// for (int i = 0; i &lt; span.Length; i++)
    /// {
    ///     _ = span[i];
    /// }
    /// </code>
    /// </example>
    public sealed class ConcurrentHashSet<T> : ISet<T>, IReadOnlyCollection<T>
    {
        private readonly ConcurrentDictionary<T, byte> _dictionary;
        private readonly IEqualityComparer<T> _comparer;

        // 0 = no null present; 1 = null present
        // int (not bool) is used because Interlocked has the widest support for int operations.
        private int _hasNull;

        /// <summary>
        /// Gets the enumeration mode that controls how <see cref="GetEnumerator"/> behaves.
        /// </summary>
        public ConcurrentHashSetEnumerationMode EnumerationMode { get; }

        /// <summary>
        /// Gets the comparer used to determine equality of elements.
        /// </summary>
        public IEqualityComparer<T> Comparer => this._comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> class.
        /// </summary>
        public ConcurrentHashSet()
            : this(comparer: null, enumerationMode: ConcurrentHashSetEnumerationMode.Live)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> class with a specified comparer.
        /// </summary>
        public ConcurrentHashSet(IEqualityComparer<T>? comparer)
            : this(comparer, ConcurrentHashSetEnumerationMode.Live)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> class with a specified enumeration mode.
        /// </summary>
        public ConcurrentHashSet(ConcurrentHashSetEnumerationMode enumerationMode)
            : this(comparer: null, enumerationMode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> class with a specified comparer and enumeration mode.
        /// </summary>
        public ConcurrentHashSet(IEqualityComparer<T>? comparer, ConcurrentHashSetEnumerationMode enumerationMode)
        {
            this._comparer = comparer ?? EqualityComparer<T>.Default;
            this._dictionary = new ConcurrentDictionary<T, byte>(this._comparer);
            this.EnumerationMode = enumerationMode;
        }

        /// <summary>
        /// Gets the number of elements contained in the set.
        /// </summary>
        /// <remarks>
        /// <see cref="ConcurrentDictionary{TKey,TValue}.Count"/> has snapshot semantics. ([Official Docs] :contentReference[oaicite:2]{index=2})
        /// </remarks>
        public int Count
        {
            get
            {
                int count = this._dictionary.Count;
                if (Volatile.Read(ref this._hasNull) != 0)
                {
                    count++;
                }

                return count;
            }
        }

        bool ICollection<T>.IsReadOnly
            => false;

        /// <summary>
        /// Adds an item to the set and returns whether it was newly added.
        /// </summary>
        public bool Add(T item)
            => TryAdd(item);

        void ICollection<T>.Add(T item)
            => TryAdd(item);

        /// <summary>
        /// Attempts to add an item to the set and returns whether it was newly added.
        /// </summary>
        public bool TryAdd(T item)
        {
            if (item is null)
            {
                return Interlocked.Exchange(ref this._hasNull, 1) == 0;
            }

            return this._dictionary.TryAdd(item, 0);
        }

        /// <summary>
        /// Removes an item from the set and returns whether it was removed.
        /// </summary>
        public bool Remove(T item)
            => TryRemove(item);

        /// <summary>
        /// Attempts to remove an item from the set and returns whether it was removed.
        /// </summary>
        public bool TryRemove(T item)
        {
            if (item is null)
            {
                return Interlocked.Exchange(ref this._hasNull, 0) != 0;
            }

            return this._dictionary.TryRemove(item, out _);
        }

        /// <summary>
        /// Determines whether the set contains a specific value.
        /// </summary>
        public bool Contains(T item)
        {
            if (item is null)
            {
                return Volatile.Read(ref this._hasNull) != 0;
            }

            return this._dictionary.ContainsKey(item);
        }

        /// <summary>
        /// Removes all items from the set.
        /// </summary>
        public void Clear()
        {
            this._dictionary.Clear();
            _ = Interlocked.Exchange(ref this._hasNull, 0);
        }

        /// <summary>
        /// Copies the elements of the set to an array, starting at a particular array index.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);

            if ((uint)arrayIndex > (uint)array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            T[] snapshot = CreateSnapshotArray();
            if (array.Length - arrayIndex < snapshot.Length)
            {
                throw new ArgumentException("Destination array is too small.");
            }

            Array.Copy(snapshot, 0, array, arrayIndex, snapshot.Length);
        }

        /// <summary>
        /// Creates a moment-in-time snapshot of the set for deterministic enumeration or bulk processing.
        /// </summary>
        /// <remarks>
        /// Internally this is built from a snapshot copy of dictionary keys. The keys collection returned by
        /// <see cref="ConcurrentDictionary{TKey,TValue}.Keys"/> is a copy and is not kept in sync. ([Official Docs] :contentReference[oaicite:3]{index=3})
        /// </remarks>
        public SnapshotView Snapshot()
            => new SnapshotView(CreateSnapshotArray());

        /// <summary>
        /// Returns an enumerator for this set based on <see cref="EnumerationMode"/>.
        /// </summary>
        public Enumerator GetEnumerator()
            => new Enumerator(this, this.EnumerationMode);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this, this.EnumerationMode);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this, this.EnumerationMode);

        /// <summary>
        /// Represents a moment-in-time snapshot of a <see cref="ConcurrentHashSet{T}"/>.
        /// </summary>
        public readonly struct SnapshotView : IReadOnlyList<T>, IEquatable<SnapshotView>
        {
            private readonly T[] _items;

            internal SnapshotView(T[] items)
            {
                this._items = items ?? throw new ArgumentNullException(nameof(items));
            }

            /// <summary>Gets the number of elements in the snapshot.</summary>
            public int Count
                => this._items.Length;

            /// <summary>Gets the snapshot element at the specified index.</summary>
            public T this[int index]
                => this._items[index];

            /// <summary>
            /// Gets a <see cref="ReadOnlySpan{T}"/> over the snapshot for allocation-free access.
            /// </summary>
            public ReadOnlySpan<T> Span
                => this._items;

            /// <summary>
            /// Gets a <see cref="ReadOnlyMemory{T}"/> over the snapshot.
            /// </summary>
            public ReadOnlyMemory<T> Memory
                => this._items;

            /// <summary>
            /// Copies the snapshot to a new array.
            /// </summary>
            public T[] ToArray()
                => (T[])this._items.Clone();

            /// <summary>Returns an enumerator for this snapshot.</summary>
            public SnapshotEnumerator GetEnumerator()
                => new SnapshotEnumerator(this._items);

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
                => new SnapshotEnumerator(this._items);
            IEnumerator IEnumerable.GetEnumerator()
                => new SnapshotEnumerator(this._items);

            public struct SnapshotEnumerator : IEnumerator<T>
            {
                private readonly T[] _items;
                private int _index;

                internal SnapshotEnumerator(T[] items)
                {
                    this._items = items;
                    this._index = -1;
                    this.Current = default!;
                }

                public T Current { get; private set; }
                object IEnumerator.Current => this.Current!;

                public bool MoveNext()
                {
                    int next = this._index + 1;
                    if ((uint)next >= (uint)this._items.Length)
                    {
                        this.Current = default!;
                        return false;
                    }

                    this._index = next;
                    this.Current = this._items[next];
                    return true;
                }

                /// <summary>
                /// Reset is not supported (typical for .NET enumerators; foreach never uses it).
                /// </summary>
                /// <exception cref="NotSupportedException">Reset is not supported (typical for .NET enumerators; foreach never uses it).</exception>
                public void Reset()
                    => throw new NotSupportedException();

                public void Dispose() { }
            }

            /// <summary>
            /// SnapshotView equality is identity-based (same captured buffer). Use <see cref="SequenceEquals(SnapshotView)"/> for content equality.
            /// </summary>
            /// <remarks>Use this equality check in hot paths where performance matters. It does not compare content for equality but instead the captured buffer for reference equality.<para/>
            /// Use <see cref="SequenceEquals(SnapshotView)"/> if performance is n ot important and content equality matters.</remarks>
            /// <param name="other"></param>
            /// <returns><see langword="true"/> if the two snapshots are equal based on their underlying captured buffers; otherwise, <see langword="false"/>.</returns>
            public bool Equals(SnapshotView other)
                => ReferenceEquals(this._items, other._items);

            /// <summary>
            /// Determines whether the current snapshot view contains the same sequence of elements as the specified
            /// snapshot view.
            /// </summary>
            /// <param name="other">The snapshot view to compare with the current instance.</param>
            /// <returns>true if the sequences of elements in both snapshot views are equal; otherwise, false.</returns>
            public bool SequenceEquals(SnapshotView other)
                => this.Span.SequenceEqual(other.Span);

            public override bool Equals(object? obj)
                => obj is SnapshotView other && Equals(other);

            public override int GetHashCode()
                => this._items is null ? 0 : RuntimeHelpers.GetHashCode(this._items);

            public static bool operator ==(SnapshotView left, SnapshotView right) => left.Equals(right);
            public static bool operator !=(SnapshotView left, SnapshotView right) => !left.Equals(right);
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly EnumeratorKind _kind;

            // Live enumerator fields:
            private readonly ConcurrentDictionary<T, byte>? _dictionary;
            private readonly IEnumerator<KeyValuePair<T, byte>>? _dictEnumerator;
            private readonly int _hasNullAtStart;
            private int _nullYielded;

            // Snapshot enumerator fields:
            private readonly T[]? _snapshot;
            private int _snapshotIndex;

            internal Enumerator(ConcurrentHashSet<T> set, ConcurrentHashSetEnumerationMode mode)
            {
                ArgumentNullException.ThrowIfNull(set);

                if (mode == ConcurrentHashSetEnumerationMode.Snapshot)
                {
                    this._kind = EnumeratorKind.Snapshot;

                    this._snapshot = set.CreateSnapshotArray();
                    this._snapshotIndex = -1;

                    this._dictionary = null;
                    this._dictEnumerator = default;

                    this._hasNullAtStart = 0;
                    this._nullYielded = 1; // irrelevant
                    this.Current = default!;

                    return;
                }

                this._kind = EnumeratorKind.Live;
                this._dictionary = set._dictionary;
                this._dictEnumerator = this._dictionary.GetEnumerator();
                this._hasNullAtStart = Volatile.Read(ref set._hasNull);
                this._nullYielded = 0;

                this._snapshot = null;
                this._snapshotIndex = -1;

                this.Current = default!;
            }

            public T Current { get; private set; }
            object IEnumerator.Current
                => this.Current!;

            public bool MoveNext()
            {
                if (this._kind == EnumeratorKind.Snapshot)
                {
                    int next = this._snapshotIndex + 1;
                    if ((uint)next >= (uint)this._snapshot!.Length)
                    {
                        this.Current = default!;
                        return false;
                    }

                    this._snapshotIndex = next;
                    this.Current = this._snapshot[next];
                    return true;
                }

                // Live: yield null first (if present at enumerator creation), then enumerate dictionary keys.
                if (this._nullYielded == 0 && this._hasNullAtStart != 0)
                {
                    this._nullYielded = 1;
                    this.Current = default!;
                    return true;
                }

                if (this._dictEnumerator.MoveNext())
                {
                    this.Current = this._dictEnumerator.Current.Key;
                    return true;
                }

                this.Current = default!;
                return false;
            }

            /// <summary>
            /// Reset is not supported (typical for .NET enumerators; foreach never uses it).
            /// </summary>
            /// <exception cref="NotSupportedException">Reset is not supported (typical for .NET enumerators; foreach never uses it).</exception>
            public void Reset()
                => throw new NotSupportedException();

            public void Dispose()
                => this._dictEnumerator.Dispose();
        }

        private enum EnumeratorKind
        {
            Live,
            Snapshot
        }

        private T[] CreateSnapshotArray()
        {
            // Keys returns a copy and is not kept in sync. ([Official Docs] :contentReference[oaicite:4]{index=4})
            ICollection<T> keysSnapshot = this._dictionary.Keys;

            bool hasNull = Volatile.Read(ref this._hasNull) != 0;
            int offset = hasNull ? 1 : 0;

            var result = new T[keysSnapshot.Count + offset];

            if (hasNull)
            {
                result[0] = default!;
            }

            keysSnapshot.CopyTo(result, offset);
            return result;
        }

        #region ISet<T> bulk ops (non-atomic)

        public void UnionWith(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            foreach (T? item in other)
            {
                _ = TryAdd(item);
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            foreach (T? item in other)
            {
                _ = TryRemove(item);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            var otherSet = new HashSet<T>(other, this._comparer);
            SnapshotView snapshot = Snapshot();

            foreach (T? item in snapshot)
            {
                if (!otherSet.Contains(item))
                {
                    _ = TryRemove(item);
                }
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            foreach (T? item in other)
            {
                if (!TryRemove(item))
                {
                    _ = TryAdd(item);
                }
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            var otherSet = new HashSet<T>(other, this._comparer);
            foreach (T? item in Snapshot())
            {
                if (!otherSet.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            foreach (T? item in other)
            {
                if (!Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            var otherSet = new HashSet<T>(other, this._comparer);
            if (this.Count <= otherSet.Count)
            {
                return false;
            }

            return IsSupersetOf(otherSet);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            var otherSet = new HashSet<T>(other, this._comparer);
            if (this.Count >= otherSet.Count)
            {
                return false;
            }

            return IsSubsetOf(otherSet);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            foreach (T? item in other)
            {
                if (Contains(item))
                {
                    return true;
                }
            }

            return false;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            var otherSet = new HashSet<T>(other, this._comparer);

            SnapshotView snapshot = Snapshot();
            if (snapshot.Count != otherSet.Count)
            {
                return false;
            }

            foreach (T? item in snapshot)
            {
                if (!otherSet.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }

    /// <summary>
    /// Controls how <see cref="ConcurrentHashSet{T}"/> enumerates.
    /// </summary>
    public enum ConcurrentHashSetEnumerationMode
    {
        /// <summary>
        /// Enumerates using the underlying <see cref="ConcurrentDictionary{TKey,TValue}"/> enumerator.
        /// Thread-safe, but not a moment-in-time snapshot. ([Official Docs] :contentReference[oaicite:5]{index=5})
        /// </summary>
        Live = 0,

        /// <summary>
        /// Enumerates a moment-in-time snapshot created at the start of enumeration.
        /// </summary>
        Snapshot = 1,

        /// <summary>
        /// Default enumeration is live; callers explicitly request a stable view via <see cref="ConcurrentHashSet{T}.Snapshot"/>.
        /// </summary>
        SnapshotOnDemand = 2,
    }
}
