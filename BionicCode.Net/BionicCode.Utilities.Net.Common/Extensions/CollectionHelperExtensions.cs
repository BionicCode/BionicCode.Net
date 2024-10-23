namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

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
      => source == null
        ? throw new ArgumentNullException(nameof(source))
        : !source.Any();

    /// <summary>
    /// Determines whether an array is empty.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <returns><see langword="true"/> if <paramref name="source"/> is empty. Otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static bool IsEmpty<TItem>(this TItem[] source)
      => source == null
        ? throw new ArgumentNullException(nameof(source))
        : source.Length == 0;

    /// <summary>
    /// Returns a range of elements.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <param name="startIndex">The inclusive starting index of the range.</param>
    /// <param name="count">The number of elements to take.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the requested range of the original <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<TItem> TakeRange<TItem>(this IEnumerable<TItem> source, int startIndex, int count)
      => source == null
        ? throw new ArgumentNullException(nameof(source))
        : source.Skip(startIndex).Take(count);

#if NETSTANDARD2_1 || NET
    /// <summary>
    /// Returns a range of elements.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <param name="range">A <see cref="Range"/> to define the range of elements to be taken.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the requested range of the original <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<TItem> TakeRange<TItem>(this IEnumerable<TItem> source, Range range)
      => source == null
        ? throw new ArgumentNullException(nameof(source))
        : source is TItem[] array ? array[range] : source.ToArray()[range];
#endif

    /// <summary>
    /// Adds a range of items to the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="source">The <see cref="ICollection{T}"/> to modify.</param>
    /// <param name="range">The items to add.</param>
    /// <remarks>Although this method returns a <see cref="IEnumerable{T}"/> it modifies the original collection. The value is only returned to enable method chaining.</remarks>
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

    ///// <summary>
    ///// Adds a range of items to the <see cref="ICollection{KeyValuePair{TKey, TItem}}"/>, allowing duplicate keys.
    ///// </summary>
    ///// <typeparam name="TItem">The type of the item.</typeparam>
    ///// <param name="collection">The <see cref="ICollection{T}"/> to modify.</param>
    ///// <param name="range">The items to add.</param>
    ///// <remarks>Use <see cref="AddRange{TKey, TValue}(IDictionary{TKey, TValue}, IEnumerable{KeyValuePair{TKey, TValue}}, AddRangeMode)"/> to disallow duplicate keys.</remarks>
    ///// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
    ///// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    //public static void AddRange<TKey, TItem>(this ICollection<KeyValuePair<TKey, TItem>> collection, IEnumerable<KeyValuePair<TKey, TItem>> range)
    //{
    //  ArgumentNullExceptionEx.ThrowIfNull(collection);
    //  ArgumentNullExceptionEx.ThrowIfNull(range);

    //  if (collection.IsReadOnly)
    //  {
    //    throw new NotSupportedException(ExceptionMessages.GetModificationOfReadOnlyCollectionNotSupportedExceptionMessage(collection));
    //  }

    //  foreach (KeyValuePair<TKey, TItem> item in range)
    //  {
    //    collection.Add(item);
    //  }
    //}

    /// <summary>
    /// Adds a <see cref="IDictionary{TKey,TValue}"/> to the <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <see cref="IDictionary{TKey,TValue}"/> to add.</param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The value is only returned to enable method chaining.</remarks>
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
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <see cref="IDictionary{TKey,TValue}"/> to add.</param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The value is only returned to enable method chaining.</remarks>
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
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <c>IEnumerable&lt;KeyValuePair&lt;TKey,TValue&gt;&gt;</c>  to add.</param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The value is only returned to enable method chaining.</remarks>
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
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <see cref="IDictionary{TKey,TValue}"/> to add.</param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The value is only returned to enable method chaining.</remarks>
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
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <c>IEnumerable&lt;(TKey Key,TValue Value&gt;&gt;</c>  to add.</param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The value is only returned to enable method chaining.</remarks>
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
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <see cref="IDictionary{TKey,TValue}"/> to add.</param>
    /// <returns>The original <see cref="IDictionary{TKey, TValue}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IDictionary{TKey, TValue}"/> it modifies the original collection. The value is only returned to enable method chaining.</remarks>
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

    public static TItem[] AddRange<TItem>(this TItem[] destination, IEnumerable<TItem> range)
      => destination.AddRange(range, destination.Length, 0, -1);

    public static TItem[] AddRange<TItem>(this TItem[] destination, IEnumerable<TItem> range, int destinationStartIndex, int rangeStartIndex, int rangeCount)
    {
      ArgumentNullExceptionEx.ThrowIfNull(destination, nameof(destination));
      ArgumentNullExceptionEx.ThrowIfNull(range, nameof(range));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationStartIndex, nameof(destinationStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeCount, nameof(rangeCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(destinationStartIndex, destination.Length, nameof(destinationStartIndex));

      if (destination.IsEmpty())
      {
        if (range is TItem[] sourceArray)
        {
          if (sourceArray.Length == rangeCount)
          {
            return sourceArray;
          }

          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, sourceArray.Length, nameof(rangeStartIndex));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeCount, sourceArray.Length - rangeStartIndex, nameof(rangeCount));

          destination = new TItem[rangeCount];
          Array.Copy(sourceArray, rangeStartIndex, destination, 0, rangeCount);

          return destination;
        }
        else if (range is IList<TItem> sourceList)
        {
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, sourceList.Count, nameof(rangeStartIndex));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeCount, sourceList.Count - rangeStartIndex, nameof(rangeCount));

          destination = new TItem[rangeCount];
          int sourceIndex = rangeStartIndex;
          for (int destinationIndex = destinationStartIndex; destinationIndex < destination.Length; destinationIndex++, sourceIndex++)
          {
            destination[destinationIndex] = sourceList[sourceIndex];
          }

          return destination;
        }
        else
        {
          sourceArray = range.ToArray();
          if (sourceArray.Length == rangeCount)
          {
            return sourceArray;
          }

          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, sourceArray.Length, nameof(rangeStartIndex));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeCount, sourceArray.Length - rangeStartIndex, nameof(rangeCount));

          destination = new TItem[rangeCount];
          Array.Copy(sourceArray, rangeStartIndex, destination, 0, rangeCount);

          return destination;
        }
      }
      else if (!range.Any())
      {
        return destination;
      }
      else
      {
        int newLength = destinationStartIndex + 1 + rangeCount;
        Array.Resize(ref destination, newLength);

        if (range is TItem[] sourceArray)
        {
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, sourceArray.Length, nameof(rangeStartIndex));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeCount, sourceArray.Length - rangeStartIndex, nameof(rangeCount));

          Array.Copy(sourceArray, rangeStartIndex, destination, destinationStartIndex, rangeCount);
        }
        else if (range is IList<TItem> sourceList)
        {
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, sourceList.Count, nameof(rangeStartIndex));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeCount, sourceList.Count - rangeStartIndex, nameof(rangeCount));

          int sourceIndex = rangeStartIndex;
          for (int destinationIndex = destinationStartIndex; destinationIndex < destination.Length; destinationIndex++, sourceIndex++)
          {
            destination[destinationIndex] = sourceList[sourceIndex];
          }
        }
        else
        {
          sourceArray = range.ToArray();
          if (sourceArray.Length == rangeCount)
          {
            return sourceArray;
          }

          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, sourceArray.Length, nameof(rangeStartIndex));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeCount, sourceArray.Length - rangeStartIndex, nameof(rangeCount));

          Array.Copy(sourceArray, rangeStartIndex, destination, destinationStartIndex, rangeCount);
        }
      }

      return destination;
    }

#if !(NETSTANDARD2_0 || NETFRAMEWORK)
    public static TItem[] AddRange<TItem>(this TItem[] destination, IEnumerable<TItem> source, Range destinationRange, Range sourceRange)
    {
      ArgumentNullExceptionEx.ThrowIfNull(destination, nameof(destination));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationRange.Start.Value, nameof(destinationRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationRange.End.Value, nameof(destinationRange.End));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceRange.Start.Value, nameof(sourceRange.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceRange.End.Value, nameof(sourceRange.End));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(destinationRange.Start.Value, destination.Length, nameof(destinationRange.Start));

      if (destination.IsEmpty())
      {
        if (source is TItem[] sourceArray)
        {
          destination = sourceArray[sourceRange];
        }
        else if (source is IList<TItem> sourceList)
        {
          int sourceCount = sourceRange.GetOffsetAndLength(sourceList.Count).Length;
          int sourceStartIndex = sourceRange.GetOffsetAndLength(sourceList.Count).Offset;
          int destinationStartIndex = destinationRange.GetOffsetAndLength(destination.Length).Offset;
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(sourceStartIndex, sourceList.Count, nameof(sourceRange.Start));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(sourceCount, sourceList.Count - sourceStartIndex, nameof(sourceRange));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(destinationStartIndex, destination.Length, nameof(destinationStartIndex));
          ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationStartIndex, nameof(destinationStartIndex));

          Array.Resize(ref destination, sourceCount);
          int sourceIndex = sourceStartIndex;
          for (int destinationIndex = destinationStartIndex; destinationIndex < destination.Length; destinationIndex++, sourceIndex++)
          {
            destination[destinationIndex] = sourceList[sourceIndex];
          }
        }
        else
        {
          sourceArray = source.ToArray();
          int sourceCount = sourceRange.GetOffsetAndLength(sourceArray.Length).Length;
          int sourceStartIndex = sourceRange.GetOffsetAndLength(sourceArray.Length).Offset;
          int destinationStartIndex = destinationRange.GetOffsetAndLength(destination.Length).Offset;

          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(sourceStartIndex, sourceArray.Length, nameof(sourceRange.Start));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(sourceCount, sourceArray.Length - sourceStartIndex, nameof(sourceRange));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(destinationStartIndex, destination.Length, nameof(destinationStartIndex));
          ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationStartIndex, nameof(destinationStartIndex));

          if (sourceArray.Length == sourceCount)
          {
            destination = sourceArray;
          }
          else
          {
            destination = sourceArray[sourceRange];
          }
        }
      }
      else if (!source.Any())
      {
        return destination;
      }
      else
      {
        if (source is TItem[] sourceArray)
        {
          int sourceCount = sourceRange.GetOffsetAndLength(sourceArray.Length).Length;
          int sourceStartIndex = sourceRange.GetOffsetAndLength(sourceArray.Length).Offset;
          int destinationStartIndex = destinationRange.GetOffsetAndLength(destination.Length).Offset;

          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(sourceStartIndex, sourceArray.Length, nameof(sourceRange.Start));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(sourceCount, sourceArray.Length - sourceStartIndex, nameof(sourceRange));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(destinationStartIndex, destination.Length, nameof(destinationStartIndex));
          ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationStartIndex, nameof(destinationStartIndex));

          int newLength = destinationStartIndex + 1 + sourceCount;
          Array.Resize(ref destination, newLength);
          Array.Copy(sourceArray[sourceRange], 0, destination, destinationStartIndex, sourceCount);
        }
        else if (source is IList<TItem> sourceList)
        {
          int sourceCount = sourceRange.GetOffsetAndLength(sourceList.Count).Length;
          int sourceStartIndex = sourceRange.GetOffsetAndLength(sourceList.Count).Offset;
          int destinationStartIndex = destinationRange.GetOffsetAndLength(destination.Length).Offset;
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(sourceStartIndex, sourceList.Count, nameof(sourceRange.Start));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(sourceCount, sourceList.Count - sourceStartIndex, nameof(sourceRange));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(destinationStartIndex, destination.Length, nameof(destinationStartIndex));
          ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationStartIndex, nameof(destinationStartIndex));

          int newLength = destinationStartIndex + 1 + sourceCount;
          Array.Resize(ref destination, newLength);
          int sourceIndex = sourceStartIndex;
          for (int destinationIndex = destinationStartIndex; destinationIndex < destination.Length; destinationIndex++, sourceIndex++)
          {
            destination[destinationIndex] = sourceList[sourceIndex];
          }
        }
        else
        {
          sourceArray = source.ToArray();
          int sourceCount = sourceRange.GetOffsetAndLength(sourceArray.Length).Length;
          int sourceStartIndex = sourceRange.GetOffsetAndLength(sourceArray.Length).Offset;
          int destinationStartIndex = destinationRange.GetOffsetAndLength(destination.Length).Offset;

          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(sourceStartIndex, sourceArray.Length, nameof(sourceRange.Start));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(sourceCount, sourceArray.Length - sourceStartIndex, nameof(sourceRange));
          ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(destinationStartIndex, destination.Length, nameof(destinationStartIndex));
          ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationStartIndex, nameof(destinationStartIndex));

          if (sourceArray.Length == sourceCount)
          {
            destination = sourceArray;
          }
          else
          {
            int newLength = destinationStartIndex + 1 + sourceCount;
            Array.Resize(ref destination, newLength);
            Array.Copy(sourceArray[sourceRange], 0, destination, destinationStartIndex, sourceCount);
          }
        }
      }

      return destination;
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
    /// <para>The performance gain is only relevant if the collection is sorted in ascending order.</para>
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
    /// <see cref="LastInSorted{TItem}(IEnumerable{TItem}, Func{TItem, bool})"/> expects a sorted collection to avoid iterating the complete collection and therefore to significantly improve the performance in terms of speed and memory footprint. 
    /// <para>The performance gain is only relevant if the collection is sorted in ascending order.</para>
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
