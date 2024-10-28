namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Text;

  public static class ArrayEx
  {
#if !(NETSTANDARD2_0 || NETFRAMEWORK)
    public static void MoveRange<TItem>(ref TItem[] array, Range range, int newIndex, bool isResizeEnabled = false)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));

      (int rangeStartIndex, int rangeLength) = range.GetOffsetAndLength(array.Length);
      int rangeEndIndex = rangeStartIndex + rangeLength;
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, array.Length, nameof(range.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeEndIndex, array.Length, nameof(range.End));

      if (!isResizeEnabled)
      {
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(newIndex, array.Length, nameof(newIndex));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(newIndex + rangeLength, array.Length, nameof(newIndex));
      }

      MoveRangeInternal(ref array, rangeStartIndex, rangeLength, newIndex, isResizeEnabled: false);
    }
#endif

    public static void MoveRange<TItem>(ref TItem[] array, int rangeStartIndex, int rangeLength, int newIndex, bool isResizeEnabled = false)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));

      int rangeEndIndex = rangeStartIndex + rangeLength;
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, array.Length, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeEndIndex, array.Length, nameof(rangeLength));

      if (!isResizeEnabled)
      {
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(newIndex, array.Length, nameof(newIndex));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(newIndex + rangeLength, array.Length, nameof(newIndex));
      }

      MoveRangeInternal(ref array, rangeStartIndex, rangeLength, newIndex, isResizeEnabled);
    }

    public static void Insert<TItem>(ref TItem[] destination, int destinationStartIndex, TItem[] sourceArray)
    {
      ArgumentNullExceptionEx.ThrowIfNull(destination, nameof(destination));
      ArgumentNullExceptionEx.ThrowIfNull(sourceArray, nameof(sourceArray));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationStartIndex, nameof(destinationStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(destinationStartIndex, destination.Length, nameof(destinationStartIndex));

      int sourceCount = sourceArray.Length;
      int desiredLength = destination.Length + sourceCount;
      bool isAddRange = destinationStartIndex == destination.Length;
      if (destination.IsEmpty())
      {
        destination = sourceArray;
      }
      else
      {
        if (isAddRange)
        {
          Array.Resize(ref destination, desiredLength);
        }
        else
        {
          ArrayEx.MoveRangeInternal(ref destination, destinationStartIndex, destination.Length, destinationStartIndex + sourceCount, isResizeEnabled: true);
        }

        Array.Copy(sourceArray, 0, destination, destinationStartIndex, sourceArray.Length);
      }
    }

    public static void Insert<TItem>(ref TItem[] destination, int destinationStartIndex, IEnumerable<TItem> source, int rangeStartIndex, int rangeLength)
    {
      ArgumentNullExceptionEx.ThrowIfNull(destination, nameof(destination));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationStartIndex, nameof(destinationStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(destinationStartIndex, destination.Length, nameof(destinationStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(rangeLength));

      int skipCount = rangeStartIndex;
      int takeCount = rangeLength - skipCount;
      using (IEnumerator<TItem> sourceEnumerator = source.GetEnumerator())
      {
        while (skipCount > 0 && sourceEnumerator.MoveNext())
        {
          skipCount--;
        }

        if (skipCount > 0)
        {
          throw new ArgumentOutOfRangeException(nameof(rangeStartIndex));
        }

        int destinationIndex = destinationStartIndex;
        bool isAddRange = destination.Length == destinationStartIndex;
        TItem[] backup = new TItem[destination.Length];
        destination.CopyTo(backup, 0);
        if (isAddRange)
        {
          int newSize = destination.Length + takeCount;
          Array.Resize(ref destination, newSize);

          while ((takeCount > 0 || takeCount < 0) && sourceEnumerator.MoveNext())
          {
            destination[destinationIndex++] = sourceEnumerator.Current;
            takeCount--;
          }
        }
        else
        {
          ArrayEx.MoveRangeInternal(ref destination, destinationStartIndex, destination.Length - destinationStartIndex, destinationStartIndex + takeCount, isResizeEnabled: true);
          while ((takeCount > 0 || takeCount < 0) && sourceEnumerator.MoveNext())
          {
            destination[destinationIndex++] = sourceEnumerator.Current;
            takeCount--;
          }
        }

        if (takeCount > 0)
        {
          destination = backup;
          throw new ArgumentOutOfRangeException(nameof(rangeLength));
        }
      }
    }

#if !(NETSTANDARD2_0 || NETFRAMEWORK)
    public static void Insert<TItem>(ref TItem[] destination, int destinationStartIndex, IEnumerable<TItem> source, Range sourceRange)
    {
      ArgumentNullExceptionEx.ThrowIfNull(destination, nameof(destination));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationStartIndex, nameof(destinationStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(destinationStartIndex, destination.Length, nameof(destinationStartIndex));

      if (sourceRange.Start.IsFromEnd || sourceRange.End.IsFromEnd)
      {
        int sourceLength;
        if (source is ICollection<TItem> genericCollection)
        {
          sourceLength = genericCollection.Count;
        }
        else if (source is ICollection collection)
        {
          sourceLength = collection.Count;
        }
        else
        {
          TItem[] sourceArray = source.ToArray();
          sourceLength = sourceArray.Length;
        }

        (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(sourceLength);
        sourceRange = rangeStartIndex..(rangeStartIndex + rangeLength);
      }

      int skipCount = sourceRange.Start.Value;
      int takeCount = sourceRange.End.Value == 0 ? -1 : sourceRange.End.Value - skipCount;
      using IEnumerator<TItem> sourceEnumerator = source.GetEnumerator();

      while (skipCount > 0 && sourceEnumerator.MoveNext())
      {
        skipCount--;
      }

      if (skipCount > 0)
      {
        throw new ArgumentOutOfRangeException(nameof(rangeStartIndex));
      }

      int destinationIndex = destinationStartIndex;
      bool isAddRange = destination.Length == destinationStartIndex;
      TItem[] backup = new TItem[destination.Length];
      destination.CopyTo(backup, 0);
      if (isAddRange)
      {
        int newSize = destination.Length + takeCount;
        Array.Resize(ref destination, newSize);

        while ((takeCount > 0 || takeCount < 0) && sourceEnumerator.MoveNext())
        {
          destination[destinationIndex++] = sourceEnumerator.Current;
          takeCount--;
        }
      }
      else
      {
        ArrayEx.MoveRangeInternal(ref destination, destinationStartIndex, destination.Length - destinationStartIndex, destinationStartIndex + takeCount, isResizeEnabled: true);
        while ((takeCount > 0 || takeCount < 0) && sourceEnumerator.MoveNext())
        {
          destination[destinationIndex++] = sourceEnumerator.Current;
          takeCount--;
        }
      }

      if (takeCount > 0)
      {
        destination = backup;
        throw new ArgumentOutOfRangeException(nameof(sourceRange));
      }
    }
#endif
    public static void InsertInternal<TItem>(ref TItem[] destination, int destinationStartIndex, IEnumerable<TItem> source, int rangeStartIndex, int rangeLength)
    {
      int skipCount = rangeStartIndex;
      int takeCount = rangeLength;
      Debug.Assert(takeCount >= -1);

      using (IEnumerator<TItem> sourceEnumerator = source.GetEnumerator())
      {
        while (skipCount > 0 && sourceEnumerator.MoveNext())
        {
          skipCount--;
        }

        if (skipCount > 0)
        {
          throw new ArgumentOutOfRangeException(nameof(rangeStartIndex));
        }

        int destinationIndex = destinationStartIndex;
        bool isAddRange = destination.Length == destinationStartIndex;
        TItem[] backup = new TItem[destination.Length];
        destination.CopyTo(backup, 0);
        if (isAddRange)
        {
          int newSize = destination.Length + takeCount;
          Array.Resize(ref destination, newSize);

          while ((takeCount > 0 || takeCount < 0) && sourceEnumerator.MoveNext())
          {
            destination[destinationIndex++] = sourceEnumerator.Current;
            takeCount--;
          }
        }
        else
        {
          ArrayEx.MoveRangeInternal(ref destination, destinationStartIndex, destination.Length - destinationStartIndex, destinationStartIndex + takeCount, isResizeEnabled: true);
          while ((takeCount > 0 || takeCount < 0) && sourceEnumerator.MoveNext())
          {
            destination[destinationIndex++] = sourceEnumerator.Current;
            takeCount--;
          }
        }

        if (takeCount > 0)
        {
          destination = backup;
          throw new ArgumentOutOfRangeException(nameof(sourceRange));
        }
      }
    }

    internal static void MoveRangeInternal<TItem>(ref TItem[] array, int rangeStartIndex, int rangeLength, int newIndex, bool isResizeEnabled)
    {
      int rangeEndIndex = rangeStartIndex + rangeLength;
      if (isResizeEnabled && newIndex + rangeLength > array.Length)
      {
        int newSize = array.Length + (newIndex + rangeLength - array.Length);
        Debug.Assert(newSize == newIndex + rangeLength);
        Array.Resize(ref array, newSize);
      }

      int insertionIndex = newIndex;
      for (int index = rangeStartIndex; index < rangeEndIndex; index++, insertionIndex++)
      {
        TItem oldValue = array[insertionIndex];
        TItem newValue = array[index];
        array[insertionIndex] = newValue;
        array[index] = oldValue;
      }
    }
  }
}
