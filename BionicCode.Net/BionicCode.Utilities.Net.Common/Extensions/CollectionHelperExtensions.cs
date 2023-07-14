namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;

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
    /// <param name="startIndex">The inclusive starting index of the range.</param>
    /// <param name="count">The number of elements to take.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the requested range of the original <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<TItem> TakeRange<TItem>(this IEnumerable<TItem> source, Range range)
      => source == null
        ? throw new ArgumentNullException(nameof(source))
        : source.Skip(range.Start.IsFromEnd ? source.Count() - 1 - range.Start.Value : range.Start.Value).Take(range.End.IsFromEnd ? source.Count() - 1 - range.End.Value : range.End.Value);
#endif

    /// <summary>
    /// Adds a range of items to the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="source">The <see cref="ICollection{T}"/> to modify.</param>
    /// <param name="range">The items to add.</param>
    /// <returns>The original <see cref="IEnumerable{T}"/> this method was invoked on to allow method chaining.</returns>
    /// <remarks>Although this method returns a <see cref="IEnumerable{T}"/> it modifies the original collection. The value is only returned to enable method chaining.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="range"/> parameter is <see langword="null"/>.</exception>
    public static IEnumerable<TItem> AddRange<TItem>(this ICollection<TItem> source, IEnumerable<TItem> range)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      if (range == null)
      {
        throw new ArgumentNullException(nameof(range));
      }

      foreach (TItem item in range)
      {
        source.Add(item);
      }

      return source;
    }

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
    public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> range)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      if (range == null)
      {
        throw new ArgumentNullException(nameof(range));
      }

      foreach (KeyValuePair<TKey, TValue> item in range)
      {
        source.Add(item);
      }

      return source;
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
    public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> range)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      if (range == null)
      {
        throw new ArgumentNullException(nameof(range));
      }

      foreach (KeyValuePair<TKey, TValue> item in range)
      {
        source.Add(item);
      }

      return source;
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
    public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<(TKey Key, TValue Value)> range)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      if (range == null)
      {
        throw new ArgumentNullException(nameof(range));
      }

      foreach ((TKey Key, TValue Value) in range)
      {
        source.Add(Key, Value);
      }

      return source;
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
    /// <para>The performance gain is only relevant if the collection is sorted in ascending order.</para>
    /// </remarks>
    public static TItem LastOrDefaultInSorted<TItem>(this IEnumerable<TItem> source, Func<TItem, bool> predicate)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      if (predicate == null)
      {
        throw new ArgumentNullException(nameof(predicate));
      }

      return TryFindLast(source, predicate, out TItem result) 
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

      if (source.IsEmpty())
      {
        throw new InvalidOperationException(ExceptionMessages.GetInvalidOperationExceptionMessage_CollectionEmpty());
      }

      return TryFindLast(source, predicate, out TItem result)
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

#endregion
  }
}
