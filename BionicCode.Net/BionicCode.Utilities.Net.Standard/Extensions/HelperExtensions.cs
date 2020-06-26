using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BionicCode.Utilities.Net.Standard.Extensions
{
  public static class HelperExtensions
  {
    #region Collection
    public static void AddRange<TItem>(this ICollection<TItem> thisCollection, IEnumerable<TItem> range)
    {
      if (thisCollection.IsReadOnly)
      {
        throw new InvalidOperationException("Unable to mutate the collection because it is read only.");
      }

      range.ToList().ForEach(thisCollection.Add);
    }

    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> thisCollection, IDictionary<TKey, TValue> range)
    {
      if (thisCollection.IsReadOnly)
      {
        throw new InvalidOperationException("Unable to mutate the collection because it is read only.");
      }

      range.ToList().ForEach(thisCollection.Add);
    }

    #endregion

    #region Stream

    public static bool HasReachedEnd(this Stream streamToCheck) =>
      (streamToCheck != null) && (streamToCheck.Position == streamToCheck.Length);

    #endregion
  }
}
