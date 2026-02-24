namespace BionicCode.Utilities.Net;

using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

/// <summary>
/// A collection of extension methods for various default types
/// </summary>
public static partial class HelperExtensionsCommon
{
    internal static FrozenSet<Type> ImmutableFrameworkCollections { get; }

    static HelperExtensionsCommon()
    {
        IEnumerable<Type> immutableTypeInterfaces = Assembly.GetAssembly(typeof(IImmutableList<>))
          .GetExportedTypes()
          .Where(type => type.Name.StartsWith("Immutable", StringComparison.Ordinal));
        ImmutableFrameworkCollections = immutableTypeInterfaces.ToFrozenSet();
    }

    /// <summary>
    /// Determines whether a sequence is empty.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <returns><see langword="true"/> if <paramref name="source"/> is empty. Otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static bool IsEmpty<TItem>(this IEnumerable<TItem> source) => !source.Any();

    /// <summary>
    /// Determines whether a sequence is empty.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <returns><see langword="true"/> if <paramref name="source"/> is empty. Otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static bool IsEmpty(this IEnumerable source)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));

        if (source is ICollection collection)
        {
            return collection.Count == 0;
        }
        else
        {
            return !source.GetEnumerator().MoveNext();
        }
    }

    /// <summary>
    /// Returns a range of elements.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <param name="startIndex">The inclusive starting index of the range.</param>
    /// <param name="count">The number of elements to take.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the requested range of the original <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<TItem> Take<TItem>(this IEnumerable<TItem> source, int startIndex, int count)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(startIndex, nameof(startIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(count, nameof(count));

        if (source.TryGetNonEnumeratedCount(out int sourceLength))
        {
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(startIndex, sourceLength, nameof(startIndex));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(count, sourceLength - startIndex, nameof(count));
        }

        using IEnumerator<TItem> enumerator = source.GetEnumerator();
        int skipCount = startIndex;
        while (skipCount > 0 && enumerator.MoveNext())
        {
            skipCount--;
        }

        if (skipCount > 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        int takeCount = count;
        while (takeCount > 0 && enumerator.MoveNext())
        {
            yield return enumerator.Current;
            takeCount--;
        }

        if (takeCount > 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
    }

    /// <summary>
    /// Returns a range of elements.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="array"></param>
    /// <param name="range">A <see cref="Range"/> to define the range of elements to be taken.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the requested range of the original <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static Span<TItem> TakeRange<TItem>(this TItem[] array, Range range)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));

        (int startIndex, int count) = range.GetOffsetAndLength(array.Length);
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(startIndex, nameof(range.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(startIndex, array.Length, nameof(range.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(count, nameof(range));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(count, array.Length - startIndex, nameof(range));

        return array.AsSpan(range);
    }

    /// Returns a range of elements.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <param name="range">A <see cref="Range"/> to define the range of elements to be taken.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the requested range of the original <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static Span<TItem> TakeRange<TItem>(this TItem[] array, int startIndex, int count)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(startIndex, nameof(startIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(startIndex, array.Length, nameof(startIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(count, nameof(count));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(count, array.Length - startIndex, nameof(count));

        return array.AsSpan(startIndex, count);
    }

    /// <summary>
    /// Adds a range of items to the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="source">The <see cref="ICollection{T}"/> to modify.</param>
    /// <param name="range">The items to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> is an immutable collection type or <paramref name="source"/> is a read-only collection.</exception>
    public static void AddRange<TItem>(this ICollection<TItem> source, IEnumerable<TItem> range)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        if (HelperExtensionsCommon.ImmutableFrameworkCollections.Contains(source.GetType()))
        {
            throw new NotSupportedException(ExceptionMessages.GetModificationOfImmutableCollectionNotSupportedExceptionMessage(source));
        }

        if (source.IsReadOnly)
        {
            throw new NotSupportedException(ExceptionMessages.GetModificationOfReadOnlyCollectionNotSupportedExceptionMessage(source));
        }

        if (source is List<TItem> sourceList)
        {
            sourceList.AddRange(range);
        }
        else
        {
            foreach (TItem item in range)
            {
                source.Add(item);
            }
        }
    }

    /// <summary>
    /// Removes a range of items to the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="source">The <see cref="ICollection{T}"/> to modify.</param>
    /// <param name="range">The items to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> is an immutable collection type or <paramref name="source"/> is a read-only collection.</exception>
    public static void RemoveRange<TItem>(this ICollection<TItem> source, IEnumerable<TItem> range)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        if (HelperExtensionsCommon.ImmutableFrameworkCollections.Contains(source.GetType()))
        {
            throw new NotSupportedException(ExceptionMessages.GetModificationOfImmutableCollectionNotSupportedExceptionMessage(source));
        }

        if (source.IsReadOnly)
        {
            throw new NotSupportedException(ExceptionMessages.GetModificationOfReadOnlyCollectionNotSupportedExceptionMessage(source));
        }

        foreach (TItem item in range)
        {
            _ = source.Remove(item);
        }
    }

    /// <summary>
    /// Adds a range of items to the <see cref="Stack{T}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="source">The <see cref="Stack{T}"/> to modify.</param>
    /// <param name="range">The items to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> is an immutable collection type or <paramref name="source"/> is a read-only collection.</exception>
    public static void AddRange<TItem>(this Stack<TItem> source, IEnumerable<TItem> range)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        if (source is IImmutableStack<TItem>)
        {
            throw new NotSupportedException(ExceptionMessages.GetModificationOfImmutableCollectionNotSupportedExceptionMessage(source));
        }

        foreach (TItem item in range)
        {
            source.Push(item);
        }
    }

    ///// <summary>
    ///// Removes a range of items to the <see cref="Stack{T}"/>.
    ///// </summary>
    ///// <typeparam name="TItem">The type of the item.</typeparam>
    ///// <param name="source">The <see cref="Stack{T}"/> to modify.</param>
    ///// <param name="range">The items to add.</param>
    ///// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    ///// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    ///// <exception cref="NotSupportedException"><paramref name="source"/> is an immutable collection type or <paramref name="source"/> is a read-only collection.</exception>
    //public static void RemoveRange<TItem>(this Stack<TItem> source, IEnumerable<TItem> range)
    //{
    //  ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
    //  ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

    //  if (source is IImmutableStack<TItem>)
    //  {
    //    throw new NotSupportedException(ExceptionMessages.GetModificationOfImmutableCollectionNotSupportedExceptionMessage(source));
    //  }

    //  var itemsToKeep = new List<TItem>();
    //  foreach (TItem itemToRemove in range)
    //  {
    //    TItem item = source.Pop();
    //    if (itemToRemove.Equals(item))
    //    {
    //      continue;
    //    }

    //    itemsToKeep.Add(item);
    //  }

    //  for (int i = itemsToKeep.Count - 1; i >= 0; i--)
    //  {
    //    TItem item = itemsToKeep[i];
    //    source.Push(item);
    //  }
    //}

    /// <summary>
    /// Adds a range of items to the <see cref="Queue{T}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="source">The <see cref="Queue{T}"/> to modify.</param>
    /// <param name="range">The items to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> is an immutable collection type or <paramref name="source"/> is a read-only collection.</exception>
    public static void AddRange<TItem>(this Queue<TItem> source, IEnumerable<TItem> range)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        if (source is IImmutableQueue<TItem>)
        {
            throw new NotSupportedException(ExceptionMessages.GetModificationOfImmutableCollectionNotSupportedExceptionMessage(source));
        }

        foreach (TItem item in range)
        {
            source.Enqueue(item);
        }
    }

    /// <summary>
    /// Adds a <see cref="IDictionary{TKey,TValue}"/> to the <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the rangeInfo.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <see cref="IDictionary{TKey,TValue}"/> to add.</param>
    /// <param name="mode"></param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The rangeInfo is only returned to enable method chaining.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> is a read-only collection.</exception>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> range, AddRangeMode mode = AddRangeMode.ThrowOnDuplicateKey)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        if (source.IsReadOnly)
        {
            throw new NotSupportedException(ExceptionMessages.GetModificationOfReadOnlyCollectionNotSupportedExceptionMessage(source));
        }

        if (mode is AddRangeMode.ThrowOnDuplicateKey)
        {
            var addedEntries = new List<KeyValuePair<TKey, TValue>>();
            try
            {
                foreach (KeyValuePair<TKey, TValue> item in range)
                {
                    source.Add(item);
                    addedEntries.Add(item);
                }
            }
            catch (ArgumentException)
            {
                source.RemoveRange(addedEntries);

                throw;
            }
        }
        else if (mode is AddRangeMode.SkipDuplicateKey)
        {
            foreach (KeyValuePair<TKey, TValue> item in range)
            {
                if (source.ContainsKey(item.Key))
                {
                    continue;
                }

                source.Add(item);
            }
        }
    }

    /// <summary>
    /// Removes a <see cref="IDictionary{TKey,TValue}"/> from the <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the rangeInfo.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <see cref="IDictionary{TKey,TValue}"/> to add.</param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The rangeInfo is only returned to enable method chaining.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> is a read-only collection.</exception>
    public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> range)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        if (source.IsReadOnly)
        {
            throw new NotSupportedException(ExceptionMessages.GetModificationOfReadOnlyCollectionNotSupportedExceptionMessage(source));
        }

        foreach (KeyValuePair<TKey, TValue> item in range)
        {
            _ = source.Remove(item.Key);
        }
    }

    /// <summary>
    /// Adds a range of <c>IEnumerable&lt;KeyValuePair&lt;TKey,TValue&gt;&gt;</c> to the <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the rangeInfo.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <c>IEnumerable&lt;KeyValuePair&lt;TKey,TValue&gt;&gt;</c>  to add.</param>
    /// <param name="mode"></param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The rangeInfo is only returned to enable method chaining.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> is a read-only collection.</exception>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> range, AddRangeMode mode = AddRangeMode.ThrowOnDuplicateKey)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        if (source.IsReadOnly)
        {
            throw new NotSupportedException(ExceptionMessages.GetModificationOfReadOnlyCollectionNotSupportedExceptionMessage(source));
        }

        if (mode is AddRangeMode.ThrowOnDuplicateKey)
        {
            var addedEntries = new List<KeyValuePair<TKey, TValue>>();
            try
            {
                foreach (KeyValuePair<TKey, TValue> item in range)
                {
                    source.Add(item);
                    addedEntries.Add(item);
                }
            }
            catch (ArgumentException)
            {
                source.RemoveRange(addedEntries);

                throw;
            }
        }
        else if (mode is AddRangeMode.SkipDuplicateKey)
        {
            foreach (KeyValuePair<TKey, TValue> item in range)
            {
                if (source.ContainsKey(item.Key))
                {
                    continue;
                }

                source.Add(item);
            }
        }
    }

    /// <summary>
    /// Removes a <see cref="IDictionary{TKey,TValue}"/> from the <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the rangeInfo.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <see cref="IDictionary{TKey,TValue}"/> to add.</param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The rangeInfo is only returned to enable method chaining.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> is a read-only collection.</exception>
    public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> range)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        if (source.IsReadOnly)
        {
            throw new NotSupportedException(ExceptionMessages.GetModificationOfReadOnlyCollectionNotSupportedExceptionMessage(source));
        }

        foreach (KeyValuePair<TKey, TValue> item in range)
        {
            _ = source.Remove(item.Key);
        }
    }

    /// <summary>
    /// Adds a range of <c>IEnumerable&lt;(TKey Key,TValue ExecuteDelegate)&gt;</c> to the <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the rangeInfo.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <c>IEnumerable&lt;(TKey Key,TValue ExecuteDelegate&gt;&gt;</c>  to add.</param>
    /// <param name="mode"></param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The rangeInfo is only returned to enable method chaining.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> is a read-only collection.</exception>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<(TKey Key, TValue Value)> range, AddRangeMode mode = AddRangeMode.ThrowOnDuplicateKey)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        if (source.IsReadOnly)
        {
            throw new NotSupportedException(ExceptionMessages.GetModificationOfReadOnlyCollectionNotSupportedExceptionMessage(source));
        }

        if (mode is AddRangeMode.ThrowOnDuplicateKey)
        {
            var addedEntries = new List<(TKey Key, TValue Value)>();
            try
            {
                foreach ((TKey Key, TValue Value) item in range)
                {
                    source.Add(item.Key, item.Value);
                    addedEntries.Add(item);
                }
            }
            catch (ArgumentException)
            {
                source.RemoveRange(addedEntries);

                throw;
            }
        }
        else if (mode is AddRangeMode.SkipDuplicateKey)
        {
            foreach ((TKey Key, TValue Value) in range)
            {
                if (source.ContainsKey(Key))
                {
                    continue;
                }

                source.Add(Key, Value);
            }
        }
    }

    /// <summary>
    /// Removes a <see cref="IDictionary{TKey,TValue}"/> from the <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the rangeInfo.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <see cref="IDictionary{TKey,TValue}"/> to add.</param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The rangeInfo is only returned to enable method chaining.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> is a read-only collection.</exception>
    public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<(TKey Key, TValue Value)> range)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        if (source.IsReadOnly)
        {
            throw new NotSupportedException(ExceptionMessages.GetModificationOfReadOnlyCollectionNotSupportedExceptionMessage(source));
        }

        foreach ((TKey Key, _) in range)
        {
            _ = source.Remove(Key);
        }
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, IEnumerable<TItem> range)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        ArrayEx.InsertInternal(ref array, array.Length, range, 0, -1);
        return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, IEnumerable<TItem> range, int rangeStartIndex, int rangeCount)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeCount, nameof(rangeCount));

        ArrayEx.InsertInternal(ref array, array.Length, range, rangeStartIndex, rangeCount);
        return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, IEnumerable<TItem> range, int rangeStartIndex, int rangeCount)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeCount, nameof(rangeCount));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(index, array.Length, nameof(index));

        ArrayEx.InsertInternal(ref array, index, range, rangeStartIndex, rangeCount);
        return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, TItem[] range)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        ArrayEx.InsertInternal(ref array, array.Length, range, 0, range.Length);
        return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, TItem[] range, int rangeStartIndex, int rangeCount)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeCount, nameof(rangeCount));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, range.Length, nameof(rangeStartIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(rangeCount, range.Length - rangeStartIndex, nameof(rangeCount));

        ArrayEx.InsertInternal(ref array, array.Length, range, rangeStartIndex, rangeCount);
        return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, TItem[] range, int rangeStartIndex, int rangeCount)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(index, array.Length, nameof(index));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, range.Length, nameof(rangeStartIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeCount, nameof(rangeCount));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(rangeCount, range.Length - rangeStartIndex, nameof(rangeCount));

        ArrayEx.InsertInternal(ref array, index, range, rangeStartIndex, rangeCount);
        return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, IList<TItem> range)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));

        ArrayEx.InsertInternal(ref array, array.Length, range, 0, range.Count);
        return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, IList<TItem> range, int rangeStartIndex, int rangeCount)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeCount, nameof(rangeCount));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, range.Count, nameof(rangeStartIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(rangeCount, range.Count - rangeStartIndex, nameof(rangeCount));

        ArrayEx.InsertInternal(ref array, array.Length, range, 0, range.Count);
        return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, IList<TItem> range, int rangeStartIndex, int rangeCount)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(range, nameof(range));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeCount, nameof(rangeCount));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(index, array.Length, nameof(index));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, range.Count, nameof(rangeStartIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(rangeCount, range.Count - rangeStartIndex, nameof(rangeCount));

        ArrayEx.InsertInternal(ref array, index, range, 0, range.Count);
        return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, TItem[] source, Range sourceRange)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));

        (int sourceStartIndex, int sourceCount) = sourceRange.GetOffsetAndLength(source.Length);
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(sourceCount, nameof(sourceCount));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(sourceStartIndex, nameof(sourceRange.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Length, nameof(sourceRange.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(sourceCount, source.Length - sourceStartIndex, nameof(sourceRange));

        ArrayEx.InsertInternal(ref array, array.Length, source, sourceStartIndex, sourceCount);
        return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, TItem[] source, Range sourceRange)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(index, array.Length, nameof(index));

        (int sourceStartIndex, int sourceCount) = sourceRange.GetOffsetAndLength(source.Length);
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(sourceCount, nameof(sourceCount));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(sourceStartIndex, nameof(sourceRange.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Length, nameof(sourceRange.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(sourceCount, source.Length - sourceStartIndex, nameof(sourceRange));

        ArrayEx.InsertInternal(ref array, index, source, sourceStartIndex, sourceCount);
        return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, IList<TItem> source, Range sourceRange)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));

        (int sourceStartIndex, int sourceCount) = sourceRange.GetOffsetAndLength(source.Count);
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(sourceCount, nameof(sourceCount));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(sourceStartIndex, nameof(sourceRange.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Count, nameof(sourceRange.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(sourceCount, source.Count - sourceStartIndex, nameof(sourceRange));

        ArrayEx.InsertInternal(ref array, array.Length, source, sourceStartIndex, sourceCount);
        return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, IList<TItem> source, Range sourceRange)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(index, array.Length, nameof(index));

        (int sourceStartIndex, int sourceCount) = sourceRange.GetOffsetAndLength(source.Count);
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(sourceCount, nameof(sourceCount));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(sourceStartIndex, nameof(sourceRange.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Count, nameof(sourceRange.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(sourceCount, source.Count - sourceStartIndex, nameof(sourceRange));

        ArrayEx.InsertInternal(ref array, index, source, sourceStartIndex, sourceCount);
        return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, ICollection<TItem> source, Range sourceRange)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));

        (int sourceStartIndex, int sourceCount) = sourceRange.GetOffsetAndLength(source.Count);
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(sourceCount, nameof(sourceCount));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(sourceStartIndex, nameof(sourceRange.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Count, nameof(sourceRange.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(sourceCount, source.Count - sourceStartIndex, nameof(sourceRange));

        ArrayEx.InsertInternal(ref array, array.Length, source, sourceStartIndex, sourceCount);
        return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, ICollection<TItem> source, Range sourceRange)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(index, array.Length, nameof(index));

        (int sourceStartIndex, int sourceCount) = sourceRange.GetOffsetAndLength(source.Count);
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(sourceCount, nameof(sourceCount));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(sourceStartIndex, nameof(sourceRange.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Count, nameof(sourceRange.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(sourceCount, source.Count - sourceStartIndex, nameof(sourceRange));

        ArrayEx.InsertInternal(ref array, index, source, sourceStartIndex, sourceCount);
        return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, IEnumerable<TItem> source, Range sourceRange)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));

        if (source is TItem[] sourceArray)
        {
            (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(sourceArray.Length);
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, sourceArray.Length, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeLength, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));

            ArrayEx.InsertInternal(ref array, array.Length, sourceArray, rangeStartIndex, rangeLength);
        }
        else if (source is IList<TItem> list)
        {
            (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(list.Count);
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, list.Count, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeLength, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));

            ArrayEx.InsertInternal(ref array, array.Length, list, rangeStartIndex, rangeLength);
        }
        else if (source is ICollection<TItem> genericCollection)
        {
            (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(genericCollection.Count);
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, genericCollection.Count, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeLength, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));

            ArrayEx.InsertInternal(ref array, array.Length, source, rangeStartIndex, rangeLength);
        }
        else if (source is ICollection collection)
        {
            (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(collection.Count);
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, collection.Count, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeLength, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));

            ArrayEx.InsertInternal(ref array, array.Length, source, rangeStartIndex, rangeLength);
        }
        else
        {
            /* Treat as plain IEnumerable of unknown length */

            // Only calculate length if really required which is when any index of the Range is relative to the collection length
            if (sourceRange.Start.IsFromEnd || sourceRange.End.IsFromEnd)
            {
                int sourceLength = source.ToArray().Length;
                (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(sourceLength);
                sourceRange = rangeStartIndex..(rangeStartIndex + rangeLength);
            }

            int takeCount = sourceRange.End.Value == 0 ? -1 : sourceRange.End.Value - sourceRange.Start.Value;

            ArrayEx.InsertInternal(ref array, array.Length, source, sourceRange.Start.Value, takeCount);
        }

        return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, IEnumerable<TItem> source, Range sourceRange)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(index, array.Length, nameof(index));

        if (source is TItem[] sourceArray)
        {
            (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(sourceArray.Length);
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, sourceArray.Length, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeLength, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(rangeLength, sourceArray.Length, nameof(sourceRange));
            ArrayEx.InsertInternal(ref array, index, sourceArray, rangeStartIndex, rangeLength);
        }
        else if (source is IList<TItem> list)
        {
            (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(list.Count);
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, list.Count, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeLength, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));
            ArrayEx.InsertInternal(ref array, index, list, rangeStartIndex, rangeLength);
        }
        else if (source is ICollection<TItem> genericCollection)
        {
            (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(genericCollection.Count);
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, genericCollection.Count, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeLength, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));

            ArrayEx.InsertInternal(ref array, index, source, rangeStartIndex, rangeLength);
        }
        else if (source is ICollection collection)
        {
            (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(collection.Count);
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, collection.Count, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(rangeLength, nameof(sourceRange));
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));

            ArrayEx.InsertInternal(ref array, index, source, rangeStartIndex, rangeLength);
        }
        else
        {
            /* Treat as plain IEnumerable of unknown length */

            // Only calculate length if really required which is when any index of the Range is relative to the collection length
            if (sourceRange.Start.IsFromEnd || sourceRange.End.IsFromEnd)
            {
                int sourceLength = source.ToArray().Length;
                (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(sourceLength);
                sourceRange = rangeStartIndex..(rangeStartIndex + rangeLength);
            }

            int takeCount = sourceRange.End.Value == 0 ? -1 : sourceRange.End.Value - sourceRange.Start.Value;

            ArrayEx.InsertInternal(ref array, index, source, sourceRange.Start.Value, takeCount);
        }

        return array;
    }

    public static TItem[] MoveRange<TItem>(this TItem[] array, Range range, int newIndex)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(array, nameof(array));

        (int rangeStartIndex, int rangeLength) = range.GetOffsetAndLength(array.Length);
        int rangeEndIndex = rangeStartIndex + rangeLength;
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeStartIndex, array.Length, nameof(range.Start));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(rangeEndIndex, array.Length, nameof(range.End));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(newIndex, array.Length, nameof(newIndex));
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThan(newIndex + rangeLength, array.Length, nameof(newIndex));

        ArrayEx.MoveInternal(ref array, rangeStartIndex, rangeLength, newIndex, isResizeEnabled: false);
        return array;
    }

    /// <summary>
    /// A non-cached version of <see cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/> for sorted collections.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <param name="predicate">A delegate to test each element for a condition.</param>
    /// <returns>The last element in a sorted collection that satisfies the <paramref name="predicate"/> delegate or <c>null</c> if no such element was found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>    
    /// <remarks>The collection is expected to be sorted. Otherwise this method can yield unexpected results. The search will stop after the last consecutive match.<br/>
    /// While the standard <see cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/> iterates and caches the complete collection in order to produce a correct result for unsorted collections,
    /// <see cref="LastOrDefaultInSorted{TItem}(IEnumerable{TItem}, Func{TItem, bool})"/> expects a sorted collection to avoid iterating the complete collection and therefore to significantly improve the performance in terms of speed and memory footprint.
    /// <para>The result is only predictable if 
    ///   <list type="bullet">
    ///     <item>the collection is sorted</item>
    ///     <item>the provided <paramref name="predicate"/> relates to the sorting criteria</item>
    ///   </list>
    /// </para>
    /// </remarks>
    public static TItem? LastOrDefaultInSorted<TItem>(this IEnumerable<TItem> source, Func<TItem, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        return TryFindLast(source, predicate, out TItem? result)
            ? result
            : default;
    }

    /// <summary>
    /// A non-cached version of <see cref="Enumerable.Last{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/> for ascending sorted collections. 
    /// <br></br>See remarks for details about the behavior and required preconditions.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <param name="predicate">A delegate to test each element for a condition.</param>
    /// <returns>The last element in a sorted collection that satisfies the <paramref name="predicate"/> delegate. If no such an element was found, a <see cref="InvalidOperationException"/> exception will be thrown.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The <paramref name="source"/> collection does not contain any element that satisfies the <paramref name="predicate"/> delegate.
    /// <para>The <paramref name="source"/> collection is empty.</para></exception>
    /// <remarks>The collection is expected to be sorted. Otherwise this method can yield unexpected results. The search will stop after the last consecutive match.<br/>
    /// While the standard <see cref="Enumerable.Last{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/> iterates and caches the complete collection in order to produce a correct result for unsorted collections,
    /// <see cref="LastInSorted{TItem}(IEnumerable{TItem}, Func{TItem, bool})"/> expects a sorted collection to avoid iterating the complete collection and therefore significantly improves the performance in terms of speed and memory complexity. 
    /// <para>The result is only predictable if 
    ///   <list type="bullet">
    ///     <item>the collection is sorted</item>
    ///     <item>the provided <paramref name="predicate"/> relates to the sorting criteria</item>
    ///   </list>
    /// </para>
    /// </remarks>
    public static TItem LastInSorted<TItem>(this IEnumerable<TItem> source, Func<TItem, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        return source.IsEmpty()
          ? throw new InvalidOperationException(ExceptionMessages.InvalidOperationExceptionMessage_CollectionEmpty)
          : TryFindLast(source, predicate, out TItem? result)
           ? result!
           : throw new InvalidOperationException(ExceptionMessages.GetInvalidOperationExceptionMessage_ItemNotFound(nameof(predicate)));
    }

    private static bool TryFindLast<TItem>(IEnumerable<TItem> source, Func<TItem, bool> predicate, out TItem? result)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        result = default;

        // Since IList supports index based access, the implementation will use 'for' instead of IEnumerator
        // to improve performance.
        if (source is IList<TItem> list)
        {
            for (int index = list.Count - 1; index >= 0; --index)
            {
                result = list[index];
                if (predicate(result))
                {
                    return true;
                }
            }

            return false;
        }

        bool isFound = false;
        foreach (TItem item in source)
        {
            if (predicate.Invoke(item))
            {
                result = item;
                isFound = true;
            }
            else if (isFound)
            {
                break;
            }
        }

        return isFound;
    }

    /// <summary>
    /// Concatenates the <see langword="string"/> representations of the elements in the sequence, using the specified separator
    /// between each element.
    /// </summary>
    /// <typeparam name="TItem">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to concatenate. Cannot be null.</param>
    /// <param name="separator">The <see langword="string"/> to use as a separator between elements. Cannot be null. The default is ", ".</param>
    /// <returns>A <see langword="string"/> that consists of the elements in the sequence delimited by the separator string. Returns an empty
    /// <see langword="string"/> if the sequence contains no elements.</returns>
    /// <remarks>If <paramref name="source"/> contains elements that are <see langword="null"/>, they are represented by
    /// the literal <see langword="null"/> in the resulting string.<para/>This method uses <see cref="string.Join(string, IEnumerable{string})"/> internally.</remarks>
    public static string JoinToString<TItem>(this IEnumerable<TItem> source, string separator = ", ")
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionAdvanced.ThrowIfNull(separator, nameof(separator));
        return string.Join(separator, source);
    }

    /// <summary>
    /// Concatenates the <see langword="string"/> representations of the elements in the sequence, using the specified separator
    /// between each element.
    /// </summary>
    /// <typeparam name="TItem">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to concatenate. Cannot be <see langword="null"/>.</param>
    /// <param name="stringTransform">A function to transform each element's string representation. Cannot be <see langword="null"/>.</param>
    /// <param name="separator">The <see langword="string"/> to use as a separator between elements. Cannot be <see langword="null"/>. The default is <c>", "</c>.</param>
    /// <returns>A <see langword="string"/> that consists of the elements in the sequence delimited by the separator string. Returns an empty
    /// <see langword="string"/> if the sequence contains no elements.</returns>
    /// <remarks>If <paramref name="source"/> contains elements that are <see langword="null"/>, they are represented by
    /// the literal <c>"null"</c> in the resulting string.<para/>This method uses <see cref="string.Join(string, IEnumerable{string})"/> internally.</remarks>
    public static string JoinToString<TItem>(this IEnumerable<TItem> source, Func<TItem, string> stringTransform, string separator = ", ")
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionAdvanced.ThrowIfNull(stringTransform, nameof(stringTransform));
        ArgumentNullExceptionAdvanced.ThrowIfNull(separator, nameof(separator));

        return string.Join(separator, source.Select(item => stringTransform(item)));
    }

    public static string JoinToString<TItem>(this ReadOnlySpan<TItem> source, string separator = ", ")
    {
        if (source.IsEmpty)
        {
            return string.Empty;
        }

        ArgumentNullExceptionAdvanced.ThrowIfNull(separator, nameof(separator));

        using PooledStringBuilder stringBuilder = StringBuilderFactory.GetOrCreate();
        for (int i = 0; i < source.Length; i++)
        {
            if (i > 0)
            {
                _ = stringBuilder.Append(separator);
            }

            _ = stringBuilder.Append(source[i]?.ToString() ?? "null");
        }

        string result = stringBuilder.ToString();
        return result;
    }

    public static string JoinToString<TItem>(this ReadOnlySpan<TItem> source, Func<TItem, string> stringTransform, string separator = ", ")
    {
        if (source.IsEmpty)
        {
            return string.Empty;
        }

        ArgumentNullExceptionAdvanced.ThrowIfNull(stringTransform, nameof(stringTransform));
        ArgumentNullExceptionAdvanced.ThrowIfNull(separator, nameof(separator));

        using PooledStringBuilder stringBuilder = StringBuilderFactory.GetOrCreate();
        for (int i = 0; i < source.Length; i++)
        {
            if (i > 0)
            {
                _ = stringBuilder.Append(separator);
            }

            ReadOnlySpan<char> value = stringTransform(source[i]);
            _ = stringBuilder.Append(value);
        }

        string result = stringBuilder.ToString();
        return result;
    }

    public static string JoinToString<TItem>(this ReadOnlySpan<TItem> source, char separator = ',')
    {
        if (source.IsEmpty)
        {
            return string.Empty;
        }

        using PooledStringBuilder stringBuilder = StringBuilderFactory.GetOrCreate();
        for (int i = 0; i < source.Length; i++)
        {
            if (i > 0)
            {
                _ = stringBuilder.Append(separator);
            }

            _ = stringBuilder.Append(source[i]?.ToString() ?? "null");
        }

        string result = stringBuilder.ToString();
        return result;
    }

    public static string JoinToString<TItem>(this ReadOnlySpan<TItem> source, Func<TItem, string> stringTransform, char separator = ',')
    {
        if (source.IsEmpty)
        {
            return string.Empty;
        }

        ArgumentNullExceptionAdvanced.ThrowIfNull(stringTransform, nameof(stringTransform));

        using PooledStringBuilder stringBuilder = StringBuilderFactory.GetOrCreate();
        for (int i = 0; i < source.Length; i++)
        {
            if (i > 0)
            {
                _ = stringBuilder.Append(separator);
            }

            ReadOnlySpan<char> value = stringTransform(source[i]);
            _ = stringBuilder.Append(value);
        }

        string result = stringBuilder.ToString();
        return result;
    }

    /// <summary>
    /// Returns the specified value if it is not <see langword="null"/>; otherwise, creates and returns a new instance of the
    /// value by invoking its parameterless constructor.
    /// </summary>
    /// <remarks>Use this method to ensure that an instance is always available, which helps prevent <see cref="NullReferenceException"/> when working with parameters or return values.</remarks>
    /// <typeparam name="T">The type of instance to validate. Must be a reference or value type that has a parameterless constructor.</typeparam>
    /// <param name="value">The value to return if it is not <see langword="null"/>; otherwise, a new instance of the specified type.</param>
    /// <returns>The original value if it is not <see langword="null"/>; otherwise, a new instance of the specified type.</returns>
    public static T OrNew<T>(this T? value) where T : class, new() => value ?? new T();

    /// <summary>
    /// Returns the specified collection if it is not <see langword="null"/>; otherwise, returns an empty collection of the same type.
    /// </summary>
    /// <remarks>This method simplifies null checks by ensuring that a collection is never null, which can
    /// help prevent <see cref="NullReferenceException"/> in client code.</remarks>
    /// <typeparam name="TCollection">The type of the collection, which must implement both <see cref="IEmptyCollectionProvider{TCollection}"/> and <see cref="IEnumerable"/>.</typeparam>
    /// <param name="source">The collection to return if it is not <see langword="null"/>.</param>
    /// <returns>The original collection if it is not <see langword="null"/>; otherwise, an empty collection of type <typeparamref name="TCollection"/>.</returns>
    public static TCollection OrEmpty<TCollection>(this TCollection? source) where TCollection : IEmptyCollectionProvider<TCollection> => source ?? TCollection.Empty;

    /// <summary>
    /// Returns the specified collection if it is not <see langword="null"/>; otherwise, returns an empty collection of the same type by invoking the provided factory <paramref name="emptyCollectionFactory"/>.
    /// </summary>
    /// <remarks>This method simplifies <see langword="null"/> checks by ensuring that a collection is never <see langword="null"/>, which can
    /// help prevent <see cref="NullReferenceException"/> in client code.</remarks>
    /// <typeparam name="TCollection">The type of the collection, which must implement <see cref="IEnumerable"/>.</typeparam>
    /// <param name="source">The collection to return if it is not <see langword="null"/>.</param>
    /// <param name="emptyCollectionFactory">A factory function to create an empty collection if the source is <see langword="null"/>.</param>
    /// <returns>The original collection if it is not <see langword="null"/>; otherwise, an empty collection of type <typeparamref name="TCollection"/> as the result of invoking the <paramref name="emptyCollectionFactory"/>.</returns>
    public static TCollection OrEmpty<TCollection>(this TCollection? source, Func<TCollection> emptyCollectionFactory) where TCollection : IEnumerable
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(emptyCollectionFactory);

        return source ?? emptyCollectionFactory.Invoke();
    }

    /// <summary>
    /// Returns the specified array if it is not <see langword="null"/>; otherwise, returns an empty array of the same type.
    /// </summary>
    /// <remarks>This method simplifies <see langword="null"/> checks by ensuring that an array is never <see langword="null"/>, which can
    /// help prevent <see cref="NullReferenceException"/> in client code.</remarks>
    /// <typeparam name="TItem">The type of the array items.</typeparam>
    /// <param name="source">The array to return if it is not <see langword="null"/>.</param>
    /// <returns>The original array if it is not <see langword="null"/>; otherwise, an empty array of type <typeparamref name="TItem"/>.</returns>
    public static TItem[] OrEmpty<TItem>(this TItem[]? source) => source ?? Array.Empty<TItem>();

    /// <summary>
    /// Returns the specified <see cref="IEnumerable{TItem}"/> if it is not <see langword="null"/>; otherwise, returns an empty <see cref="IEnumerable{TItem}"/>.
    /// </summary>
    /// <remarks>This method simplifies <see langword="null"/> checks by ensuring that a <see cref="IEnumerable{TItem}"/> is never <see langword="null"/>, which can
    /// help prevent <see cref="NullReferenceException"/> in client code.</remarks>
    /// <typeparam name="TItem">The type of the <see cref="IEnumerable{TItem}"/> items..</typeparam>
    /// <param name="source">The <see cref="IEnumerable{TItem}"/> to return if it is not <see langword="null"/>.</param>
    /// <returns>The original <see cref="IEnumerable{TItem}"/> if it is not <see langword="null"/>; otherwise, an empty <see cref="IEnumerable{TItem}"/>.</returns>
    public static IEnumerable<TItem> OrEmpty<TItem>(this IEnumerable<TItem>? source) => source ?? Enumerable.Empty<TItem>();

    /// <summary>
    /// Returns the specified <see cref="List{TItem}"/> if it is not <see langword="null"/>; otherwise, returns an empty <see cref="List{TItem}"/>.
    /// </summary>
    /// <remarks>This method simplifies <see langword="null"/> checks by ensuring that a <see cref="List{TItem}"/> is never <see langword="null"/>, which can
    /// help prevent <see cref="NullReferenceException"/> in client code.</remarks>
    /// <typeparam name="TItem">The type of the <see cref="List{TItem}"/> items..</typeparam>
    /// <param name="source">The <see cref="List{TItem}"/> to return if it is not <see langword="null"/>.</param>
    /// <returns>The original <see cref="List{TItem}"/> if it is not <see langword="null"/>; otherwise, an empty <see cref="List{TItem}"/>.</returns>
    public static List<TItem> OrEmpty<TItem>(this List<TItem>? source) => source ?? [];

    /// <summary>
    /// Determines whether the specified collection contains duplicate elements.
    /// </summary>
    /// <typeparam name="TItem">The type of the elements in the collection.</typeparam>
    /// <param name="items">The collection to check for duplicates.</param>
    /// <param name="equalityComparer">The equality comparer to use for comparing elements.</param>
    /// <returns><see langword="true"/> if the collection contains duplicate elements; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
    public static bool HasDuplicates<TItem>(this IEnumerable<TItem> items, IEqualityComparer<TItem>? equalityComparer = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        equalityComparer ??= EqualityComparer<TItem>.Default;

        var set = new HashSet<TItem>(equalityComparer);
        foreach (TItem item in items)
        {
            if (!set.Add(item))
            {
                return true;
            }
        }

        return false;
    }
}

