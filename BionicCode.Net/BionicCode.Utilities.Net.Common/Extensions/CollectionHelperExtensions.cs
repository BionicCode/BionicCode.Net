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
    /// <returns></returns>
    public static bool IsEmpty<TItem>(this IEnumerable<TItem> source)
      => !source.Any();

    /// <summary>
    /// Returns a range of elements.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <param name="startIndex">The inclusive starting index of the range.</param>
    /// <param name="count">The number of elements to take.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the requested range of the original <paramref name="source"/>.</returns>
    public static IEnumerable<TItem> TakeRange<TItem>(this IEnumerable<TItem> source, int startIndex, int count)
      => source.Skip(startIndex).Take(count);

    /// <summary>
    /// Adds a range of items to the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="thisCollection">The <see cref="ICollection{T}"/> to modify.</param>
    /// <param name="range">The items to add.</param>
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

    /// <summary>
    /// A non-cached version of <see cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/> for sorted collections.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <param name="predicate">A delegate to test each element for a condition.</param>
    /// <returns>The last element in a sorted collection that satisfies the <paramref name="predicate"/> delegate or <c>null</c> if no such element was found.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="predicate"/> is <c><see langword="null"/></c>.</exception>
    /// <exception cref="InvalidOperationException">The collection does not contain any element that satisfies the <paramref name="predicate"/> delegate.</exception>
    /// <remarks>The collection is expected to be sorted. Otherwise this method can yield unexpected results. <br/>
    /// While the standard <see cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/> iterates and caches the complete collection in order to produce a correct result for unsorted collections,
    /// <see cref="LastOrDefaultInSorted{TItem}(IEnumerable{TItem}, Func{TItem, bool})"/> expects a sorted collection to avoid iterating the complete collection and therefore to significantly improve the performance in terms of speed and memory footprint.
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

      TItem result = default;
      foreach (var item in source)
      {
        if (predicate.Invoke(item))
        {
          result = item;
        }
        else
        {
          break;
        }
      }
      return result;
    }

    /// <summary>
    /// A non-cached version of <see cref="Enumerable.Last{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/> for sorted collections.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="source"></param>
    /// <param name="predicate">A delegate to test each element for a condition.</param>
    /// <returns>The last element in a sorted collection that satisfies the <paramref name="predicate"/> delegate. If no such an element was found, a <see cref="InvalidOperationException"/> exception will be thrown.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="predicate"/> is <c><see langword="null"/></c>.</exception>
    /// <exception cref="InvalidOperationException">The collection does not contain any element that satisfies the <paramref name="predicate"/> delegate.</exception>
    /// <remarks>The collection is expected to be sorted. Otherwise this method can yield unexpected results. <br/>
    /// While the standard <see cref="Enumerable.Last{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/> iterates and caches the complete collection in order to produce a correct result for unsorted collections,
    /// <see cref="LastInSorted{TItem}(IEnumerable{TItem}, Func{TItem, bool})"/> expects a sorted collection to avoid iterating the complete collection and therefore to significantly improve the performance in terms of speed and memory footprint.
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

      TItem result = default;
      bool isFound = false;
      foreach (var item in source)
      {
        if (predicate.Invoke(item))
        {
          result = item;
          isFound = true;
        }
        else
        {
          break;
        }
      }
      if (isFound)
      {
        return result;
      }

      throw new InvalidOperationException("No element satisfies the condition in predicate or the source sequence is empty.");
    }

    /// <summary>
    /// Coverts any type to a <see cref="Dictionary{TKey,TValue}"/>, where the <c>TKey</c> is the member name and <c>TValue</c> the member's value.
    /// </summary>
    /// <param name="instanceToConvert"></param>
    /// <returns>A <see cref="Dictionary{TKey,TValue}"/>, where the <c>TKey</c> is the member name of type <see cref="string"/> and <c>TValue</c> the member's value of type <see cref="object"/>.</returns>
    public static Dictionary<string, object> ToDictionary(this object instanceToConvert)
    {
      Dictionary<string, object> resultDictionary = instanceToConvert.GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        .Where(propertyInfo => !propertyInfo.GetIndexParameters().Any())
        .ToDictionary(
          propertyInfo => propertyInfo.Name,
          propertyInfo => HelperExtensionsCommon.ConvertPropertyToDictionary(propertyInfo, instanceToConvert));
      resultDictionary.Add("IsCollection", false);
      return resultDictionary;
    }

    private static object ConvertPropertyToDictionary(PropertyInfo propertyInfo, object owner)
    {
      Type propertyType = propertyInfo.PropertyType;
      object propertyValue = propertyInfo.GetValue(owner);

      if (propertyValue is Type)
      {
        return propertyValue;
      }

      // If property is a collection don't traverse collection properties but the items instead
      if (!propertyType.Equals(typeof(string)) && typeof(IEnumerable).IsAssignableFrom(propertyType))
      {
        var items = new Dictionary<string, object>();
        var enumerable = propertyInfo.GetValue(owner) as IEnumerable;
        int index = 0;
        foreach (object item in enumerable)
        {
          // If property is a string stop traversal
          if (item.GetType().IsPrimitive || item is string)
          {
            items.Add(index.ToString(), item);
          }
          else if (item is IEnumerable enumerableItem)
          {
            items.Add(index.ToString(), HelperExtensionsCommon.ConvertIEnumerableToDictionary(enumerableItem));
          }
          else
          {
            Dictionary<string, object> dictionary = item.ToDictionary();
            items.Add(index.ToString(), dictionary);
          }

          index++;
        }

        items.Add("IsCollection", true);
        items.Add("Count", index);
        return items;
      }

      // If property is a string stop traversal
      if (propertyType.IsPrimitive || propertyType.Equals(typeof(string)))
      {
        return propertyValue;
      }

      PropertyInfo[] properties =
        propertyType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
      if (properties.Any())
      {
        Dictionary<string, object> resultDictionary = properties.ToDictionary(
          subtypePropertyInfo => subtypePropertyInfo.Name,
          subtypePropertyInfo => propertyValue == null
            ? null
            : HelperExtensionsCommon.ConvertPropertyToDictionary(subtypePropertyInfo, propertyValue));
        resultDictionary.Add("IsCollection", false);
        return resultDictionary;
      }

      return propertyValue;
    }

    private static Dictionary<string, object> ConvertIEnumerableToDictionary(IEnumerable enumerable)
    {
      var items = new Dictionary<string, object>();
      int index = 0;
      foreach (object item in enumerable)
      {
        // If property is a string stop traversal
        if (item.GetType().IsPrimitive || item is string)
        {
          items.Add(index.ToString(), item);
        }
        else
        {
          Dictionary<string, object> dictionary = item.ToDictionary();
          items.Add(index.ToString(), dictionary);
        }

        index++;
      }

      items.Add("IsCollection", true);
      items.Add("Count", index);
      return items;
    }

    #endregion
  }
}
