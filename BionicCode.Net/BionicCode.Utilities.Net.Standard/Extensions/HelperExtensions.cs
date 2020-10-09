using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BionicCode.Utilities.Net.Standard.Extensions
{
  /// <summary>
  /// A collection of extension methods for various default types
  /// </summary>
  public static class HelperExtensions
  {
    /// <summary>
    /// Adds a range of items to the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="thisCollection">The <see cref="ICollection{T}"/> to modify.</param>
    /// <param name="range">The items to add.</param>
    #region Collection
    public static IEnumerable<TItem> AddRange<TItem>(this ICollection<TItem> thisCollection, IEnumerable<TItem> range)
    {
      if (thisCollection.IsReadOnly)
      {
        throw new InvalidOperationException("Unable to mutate the collection because it is read only.");
      }

      range.ToList().ForEach(thisCollection.Add);
      return thisCollection;
    }

    /// <summary>
    /// Adds a <see cref="IDictionary{TKey,TValue}"/> to the <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="thisCollection">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <see cref="IDictionary{TKey,TValue}"/> to add.</param>
    public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> thisCollection, IDictionary<TKey, TValue> range)
    {
      if (thisCollection.IsReadOnly)
      {
        throw new InvalidOperationException("Unable to mutate the collection because it is read only.");
      }

      range.ToList().ForEach(thisCollection.Add);
      return thisCollection;
    }

    /// <summary>
    /// Adds a range of <c>IEnumerable&lt;KeyValuePair&lt;TKey,TValue&gt;&gt;</c>  to the <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="thisCollection">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <c>IEnumerable&lt;KeyValuePair&lt;TKey,TValue&gt;&gt;</c>  to add.</param>
    public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> thisCollection, IEnumerable<KeyValuePair<TKey, TValue>> range)
    {
      if (thisCollection.IsReadOnly)
      {
        throw new InvalidOperationException("Unable to mutate the collection because it is read only.");
      }

      range.ToList().ForEach(thisCollection.Add);
      return thisCollection;
    }

    /// <summary>
    /// Adds a range of <c>IEnumerable&lt;(TKey Key,TValue Value)&gt;</c> to the <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="thisCollection">The <see cref="IDictionary{TKey,TValue}"/> to modify.</param>
    /// <param name="range">The <c>IEnumerable&lt;(TKey Key,TValue Value&gt;&gt;</c>  to add.</param>
    public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> thisCollection, IEnumerable<(TKey Key, TValue Value)> range)
    {
      if (thisCollection.IsReadOnly)
      {
        throw new InvalidOperationException("Unable to mutate the collection because it is read only.");
      }

      range
        .Select(entry => new KeyValuePair<TKey, TValue>(entry.Key, entry.Value))
        .ToList()
        .ForEach(thisCollection.Add);
      return thisCollection;
    }

    #endregion

    #region Stream
    /// <summary>
    /// Return whether the end of a <see cref="Stream"/> is reached.
    /// </summary>
    /// <param name="streamToCheck"></param>
    /// <returns></returns>
    public static bool HasReachedEnd(this Stream streamToCheck) =>
      streamToCheck != null && streamToCheck.Position == streamToCheck.Length;

    #endregion
  }
}
