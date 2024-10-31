namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq;
  using System.Reflection;
  using System.Text;
  using Microsoft.CodeAnalysis.CSharp.Syntax;

  /// <summary>
  /// A collection of extension methods for various default types
  /// </summary>
  public static partial class HelperExtensionsCommon
  {
    #region Collection

    /// <summary>
    /// Determines whether a sequence is empty.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <returns><see langword="true"/> if <paramref name="source"/> is empty. Otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static bool IsEmpty<TItem>(this IEnumerable<TItem> source)
      => !source.Any();

    /// <summary>
    /// Determines whether a sequence is empty.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <returns><see langword="true"/> if <paramref name="source"/> is empty. Otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static bool IsEmpty(this IEnumerable source)
    {
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));

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
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(startIndex, nameof(startIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(count, nameof(count));

#if NET6_0_OR_GREATER
      if (source.TryGetNonEnumeratedCount(out int sourceLength))
      {
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(startIndex, sourceLength, nameof(startIndex));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(count, sourceLength - startIndex, nameof(count));
      }
#else
      if (source is ICollection<TItem> collection)
      {
        int sourceLength = collection.Count;
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(startIndex, sourceLength, nameof(startIndex));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(count, sourceLength - startIndex, nameof(count));
      }
      else if (source is TItem[] array)
      {
        int sourceLength = array.Length;
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(startIndex, sourceLength, nameof(startIndex));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(count, sourceLength - startIndex, nameof(count));
      }
#endif

      using (IEnumerator<TItem> enumerator = source.GetEnumerator())
      {
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
    }

#if !(NET6_0_OR_GREATER || NETFRAMEWORK || NETSTANDARD2_0)
    /// <summary>
    /// Returns a range of elements.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <param name="range">A <see cref="Range"/> to define the range of elements to be taken.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the requested range of the original <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<TItem> Take<TItem>(this IEnumerable<TItem> source, Range range)
    {
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));

      if (source is ICollection<TItem> collection)
      {
        int sourceLength = collection.Count;
        (int startIndex, int count) = range.GetOffsetAndLength(sourceLength);
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(startIndex, nameof(startIndex));
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(count, nameof(count));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(startIndex, sourceLength, nameof(range.Start));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(count, sourceLength - startIndex, nameof(range));

        foreach (TItem item in source.Take(startIndex, count))
        {
          yield return item;
        }
      }
      else if (source is TItem[] array)
      {
        int sourceLength = array.Length;
        (int startIndex, int count) = range.GetOffsetAndLength(sourceLength);
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(startIndex, nameof(startIndex));
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(count, nameof(count));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(startIndex, sourceLength, nameof(range.Start));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(count, sourceLength - startIndex, nameof(range));

        foreach (TItem item in source.Take(startIndex, count))
        {
          yield return item;
        }
      }
      else
      {
        if (range.Start.IsFromEnd || range.End.IsFromEnd)
        {
          TItem[] sourceArray = source.ToArray();
          (int Offset, int Length) = range.GetOffsetAndLength(sourceArray.Length);
          int startIndex = Offset;
          for (int index = startIndex; index < startIndex + Length; index++)
          {
            yield return sourceArray[index];
          }
        }
        else
        {
          int skipCount = range.Start.Value;
          int takeCount = range.End.Value - range.Start.Value + 1;
          using IEnumerator<TItem> sourceEnumerator = source.GetEnumerator();
          while (skipCount > 0 && sourceEnumerator.MoveNext())
          {
            skipCount--;
          }

          if (skipCount > 0)
          {
            throw new ArgumentOutOfRangeException(nameof(range));
          }

          while (takeCount > 0 && sourceEnumerator.MoveNext())
          {
            takeCount--;
            yield return sourceEnumerator.Current;
          }

          if (takeCount > 0)
          {
            throw new ArgumentOutOfRangeException(nameof(range));
          }
        }
      }
    }
#endif

#if !(NETSTANDARD2_0 || NETFRAMEWORK)
      /// <summary>
      /// Returns a range of elements.
      /// </summary>
      /// <typeparam name="TItem"></typeparam>
      /// <param name="source"></param>
      /// <param name="range">A <see cref="Range"/> to define the range of elements to be taken.</param>
      /// <returns>An <see cref="IEnumerable{T}"/> that contains the requested range of the original <paramref name="source"/>.</returns>
      /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
      public static Span<TItem> TakeRange<TItem>(this TItem[] array, Range range)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));

      (int startIndex, int count) = range.GetOffsetAndLength(array.Length);
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(startIndex, nameof(range.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(startIndex, array.Length, nameof(range.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(count, nameof(range));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(count, array.Length - startIndex, nameof(range));

      return array.AsSpan(range);
    }
#endif

    /// Returns a range of elements.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <param name="range">A <see cref="Range"/> to define the range of elements to be taken.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the requested range of the original <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static Span<TItem> TakeRange<TItem>(this TItem[] array, int startIndex, int count)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(startIndex, nameof(startIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(startIndex, array.Length, nameof(startIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(count, nameof(count));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(count, array.Length - startIndex, nameof(count));

      return array.AsSpan(startIndex, count);
    }

    /// <summary>
    /// Adds a range of items to the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="source">The <see cref="ICollection{T}"/> to modify.</param>
    /// <param name="range">The items to add.</param>
    /// <remarks>Although this method returns a <see cref="IEnumerable{T}"/> it modifies the original collection. The rangeInfo is only returned to enable method chaining.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    public static void AddRange<TItem>(this ICollection<TItem> source, IEnumerable<TItem> range)
    {
      ArgumentNullExceptionEx.ThrowIfNull(source);
      ArgumentNullExceptionEx.ThrowIfNull(range);

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
    /// Adds a <see cref="IDictionary{TKey,TValue}"/> to the <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the rangeInfo.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <see cref="IDictionary{TKey,TValue}"/> to add.</param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The rangeInfo is only returned to enable method chaining.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> range, AddRangeMode mode = AddRangeMode.ThrowOnDuplicateKey)
    {
      ArgumentNullExceptionEx.ThrowIfNull(source);
      ArgumentNullExceptionEx.ThrowIfNull(range);

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
    public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> range)
    {
      ArgumentNullExceptionEx.ThrowIfNull(source);
      ArgumentNullExceptionEx.ThrowIfNull(range);

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
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The rangeInfo is only returned to enable method chaining.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> range, AddRangeMode mode = AddRangeMode.ThrowOnDuplicateKey)
    {
      ArgumentNullExceptionEx.ThrowIfNull(source);
      ArgumentNullExceptionEx.ThrowIfNull(range);

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
    public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> range)
    {
      ArgumentNullExceptionEx.ThrowIfNull(source);
      ArgumentNullExceptionEx.ThrowIfNull(range);

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
    /// Adds a range of <c>IEnumerable&lt;(TKey Key,TValue Value)&gt;</c> to the <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the rangeInfo.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <c>IEnumerable&lt;(TKey Key,TValue Value&gt;&gt;</c>  to add.</param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The rangeInfo is only returned to enable method chaining.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<(TKey Key, TValue Value)> range, AddRangeMode mode = AddRangeMode.ThrowOnDuplicateKey)
    {
      ArgumentNullExceptionEx.ThrowIfNull(source);
      ArgumentNullExceptionEx.ThrowIfNull(range);

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
    public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<(TKey Key, TValue Value)> range)
    {
      ArgumentNullExceptionEx.ThrowIfNull(source);
      ArgumentNullExceptionEx.ThrowIfNull(range);

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
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(range, nameof(range));

      ArrayEx.InsertInternal(ref array, array.Length, range, 0, -1);
      return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, IEnumerable<TItem> range, int rangeStartIndex, int rangeCount)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(range, nameof(range));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeCount, nameof(rangeCount));

      ArrayEx.InsertInternal(ref array, array.Length, range, rangeStartIndex, rangeCount);
      return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, IEnumerable<TItem> range, int rangeStartIndex, int rangeCount)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(range, nameof(range));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeCount, nameof(rangeCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(index, array.Length, nameof(index));

      ArrayEx.InsertInternal(ref array, index, range, rangeStartIndex, rangeCount);
      return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, TItem[] range)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(range, nameof(range));

      ArrayEx.InsertInternal(ref array, array.Length, range, 0, range.Length);
      return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, TItem[] range, int rangeStartIndex, int rangeCount)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(range, nameof(range));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeCount, nameof(rangeCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, range.Length, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeCount, range.Length - rangeStartIndex, nameof(rangeCount));

      ArrayEx.InsertInternal(ref array, array.Length, range, rangeStartIndex, rangeCount);
      return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, TItem[] range, int rangeStartIndex, int rangeCount)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(range, nameof(range));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(index, array.Length, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, range.Length, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeCount, nameof(rangeCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeCount, range.Length - rangeStartIndex, nameof(rangeCount));

      ArrayEx.InsertInternal(ref array, index, range, rangeStartIndex, rangeCount);
      return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, IList<TItem> range)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(range, nameof(range));

      ArrayEx.InsertInternal(ref array, array.Length, range, 0, range.Count);
      return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, IList<TItem> range, int rangeStartIndex, int rangeCount)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(range, nameof(range));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeCount, nameof(rangeCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, range.Count, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeCount, range.Count - rangeStartIndex, nameof(rangeCount));

      ArrayEx.InsertInternal(ref array, array.Length, range, 0, range.Count);
      return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, IList<TItem> range, int rangeStartIndex, int rangeCount)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(range, nameof(range));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeCount, nameof(rangeCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(index, array.Length, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, range.Count, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeCount, range.Count - rangeStartIndex, nameof(rangeCount));

      ArrayEx.InsertInternal(ref array, index, range, 0, range.Count);
      return array;
    }

#if !(NETSTANDARD2_0 || NETFRAMEWORK)

    public static TItem[] AddRange<TItem>(this TItem[] array, TItem[] source, Range sourceRange)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));

      (int sourceStartIndex, int sourceCount) = sourceRange.GetOffsetAndLength(source.Length);
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceCount, nameof(sourceCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceStartIndex, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Length, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(sourceCount, source.Length - sourceStartIndex, nameof(sourceRange));

      ArrayEx.InsertInternal(ref array, array.Length, source, sourceStartIndex, sourceCount);
      return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, TItem[] source, Range sourceRange)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(index, array.Length, nameof(index));

      (int sourceStartIndex, int sourceCount) = sourceRange.GetOffsetAndLength(source.Length);
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceCount, nameof(sourceCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceStartIndex, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Length, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(sourceCount, source.Length - sourceStartIndex, nameof(sourceRange));

      ArrayEx.InsertInternal(ref array, index, source, sourceStartIndex, sourceCount);
      return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, IList<TItem> source, Range sourceRange)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));

      (int sourceStartIndex, int sourceCount) = sourceRange.GetOffsetAndLength(source.Count);
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceCount, nameof(sourceCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceStartIndex, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Count, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(sourceCount, source.Count - sourceStartIndex, nameof(sourceRange));

      ArrayEx.InsertInternal(ref array, array.Length, source, sourceStartIndex, sourceCount);
      return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, IList<TItem> source, Range sourceRange)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(index, array.Length, nameof(index));

      (int sourceStartIndex, int sourceCount) = sourceRange.GetOffsetAndLength(source.Count);
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceCount, nameof(sourceCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceStartIndex, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Count, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(sourceCount, source.Count - sourceStartIndex, nameof(sourceRange));

      ArrayEx.InsertInternal(ref array, index, source, sourceStartIndex, sourceCount);
      return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, ICollection<TItem> source, Range sourceRange)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));

      (int sourceStartIndex, int sourceCount) = sourceRange.GetOffsetAndLength(source.Count);
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceCount, nameof(sourceCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceStartIndex, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Count, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(sourceCount, source.Count - sourceStartIndex, nameof(sourceRange));

      ArrayEx.InsertInternal(ref array, array.Length, source, sourceStartIndex, sourceCount);
      return array;
    }

    public static TItem[] InsertRange<TItem>(this TItem[] array, int index, ICollection<TItem> source, Range sourceRange)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(index, array.Length, nameof(index));

      (int sourceStartIndex, int sourceCount) = sourceRange.GetOffsetAndLength(source.Count);
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceCount, nameof(sourceCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceStartIndex, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Count, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(sourceCount, source.Count - sourceStartIndex, nameof(sourceRange));

      ArrayEx.InsertInternal(ref array, index, source, sourceStartIndex, sourceCount);
      return array;
    }

    public static TItem[] AddRange<TItem>(this TItem[] array, IEnumerable<TItem> source, Range sourceRange)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));

      if (source is TItem[] sourceArray)
      {
        (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(sourceArray.Length);
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, sourceArray.Length, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));

        ArrayEx.InsertInternal(ref array, array.Length, sourceArray, rangeStartIndex, rangeLength);
      }
      else if (source is IList<TItem> list)
      {
        (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(list.Count);
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, list.Count, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));

        ArrayEx.InsertInternal(ref array, array.Length, list, rangeStartIndex, rangeLength);
      }
      else if (source is ICollection<TItem> genericCollection)
      {
        (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(genericCollection.Count);
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, genericCollection.Count, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));

        ArrayEx.InsertInternal(ref array, array.Length, source, rangeStartIndex, rangeLength);
      }
      else if (source is ICollection collection)
      {
        (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(collection.Count);
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, collection.Count, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));

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
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(index, array.Length, nameof(index));

      if (source is TItem[] sourceArray)
      {
        (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(sourceArray.Length);
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, sourceArray.Length, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, sourceArray.Length, nameof(sourceRange));
        ArrayEx.InsertInternal(ref array, index, sourceArray, rangeStartIndex, rangeLength);
      }
      else if (source is IList<TItem> list)
      {
        (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(list.Count);
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, list.Count, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));
        ArrayEx.InsertInternal(ref array, index, list, rangeStartIndex, rangeLength);
      }
      else if (source is ICollection<TItem> genericCollection)
      {
        (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(genericCollection.Count);
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, genericCollection.Count, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));

        ArrayEx.InsertInternal(ref array, index, source, rangeStartIndex, rangeLength);
      }
      else if (source is ICollection collection)
      {
        (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(collection.Count);
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, collection.Count, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, array.Length, nameof(sourceRange));

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
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));

      (int rangeStartIndex, int rangeLength) = range.GetOffsetAndLength(array.Length);
      int rangeEndIndex = rangeStartIndex + rangeLength;
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, array.Length, nameof(range.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeEndIndex, array.Length, nameof(range.End));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(newIndex, array.Length, nameof(newIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(newIndex + rangeLength, array.Length, nameof(newIndex));

      ArrayEx.MoveInternal(ref array, rangeStartIndex, rangeLength, newIndex, isResizeEnabled: false);
      return array;
    }

#endif

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
    public static TItem LastOrDefaultInSorted<TItem>(this IEnumerable<TItem> source, Func<TItem, bool> predicate)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      return predicate == null
        ? throw new ArgumentNullException(nameof(predicate))
        : TryFindLast(source, predicate, out TItem result)
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
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      if (predicate == null)
      {
        throw new ArgumentNullException(nameof(predicate));
      }

      return source.IsEmpty()
        ? throw new InvalidOperationException(ExceptionMessages.GetInvalidOperationExceptionMessage_CollectionEmpty())
        : TryFindLast(source, predicate, out TItem result)
         ? result
         : throw new InvalidOperationException(ExceptionMessages.GetInvalidOperationExceptionMessage_ItemNotFound(nameof(predicate)));
    }

    private static bool TryFindLast<TItem>(IEnumerable<TItem> source, Func<TItem, bool> predicate, out TItem result)
    {
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

#if !NET8_0_OR_GREATER
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
      => source.ToDictionary(entry => entry.Key, entry => entry.Value);

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<(TKey Key, TValue Value)> source)
      => source.ToDictionary(entry => entry.Key, entry => entry.Value);
#endif

#endregion
  }

  public enum AddRangeMode
  {
    ThrowOnDuplicateKey,
    SkipDuplicateKey,
  }
}
