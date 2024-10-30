namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Text;

  public static class ArrayEx
  {
    public static void Move<TItem>(ref TItem[] array, int oldIndex, int newIndex)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(oldIndex, nameof(oldIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(oldIndex, array.Length, nameof(oldIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(newIndex, nameof(newIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(newIndex, array.Length, nameof(newIndex));

      if (newIndex == oldIndex || array.IsEmpty())
      {
        return;
      }

      TItem item = array[oldIndex];
      int numberOfShifts = 1;
      if (newIndex > oldIndex)
      {
        int rangeStartIndex = oldIndex + 1;
        int rangeLength = newIndex - rangeStartIndex;
        ShiftRangeLeftInternal(in array, rangeStartIndex, rangeLength, numberOfShifts);
      }
      else if (newIndex < oldIndex)
      {
        int rangeStartIndex = newIndex;
        int rangeLength = oldIndex - 1 - newIndex;
        ShiftRangeRightInternal(array, rangeStartIndex, rangeLength, numberOfShifts);
      }

      array[newIndex] = item;
    }

    public static void Move<TItem>(ref TItem[] array, int oldIndex, int newIndex, bool isMoveOutOfBoundsAllowed)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(oldIndex, nameof(oldIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(oldIndex, array.Length, nameof(oldIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(newIndex, nameof(newIndex));
      if (!isMoveOutOfBoundsAllowed)
      {
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(newIndex, array.Length, nameof(newIndex));
      }

      if (newIndex == oldIndex || array.IsEmpty())
      {
        return;
      }

      if (isMoveOutOfBoundsAllowed && newIndex >= array.Length)
      {
        Array.Resize(ref array, newIndex + 1);
      }

      TItem item = array[oldIndex];
      int numberOfShifts = 1;
      if (newIndex > oldIndex)
      {
        int rangeStartIndex = oldIndex + 1;
        int rangeLength = newIndex - rangeStartIndex;
        ShiftRangeLeftInternal(in array, rangeStartIndex, rangeLength, numberOfShifts);
      }
      else if (newIndex < oldIndex)
      {
        int rangeStartIndex = newIndex;
        int rangeLength = oldIndex - 1 - newIndex;
        ShiftRangeRightInternal(array, rangeStartIndex, rangeLength, numberOfShifts);
      }

      array[newIndex] = item;
    }

    public static void ShiftRangeLeft<TItem>(in TItem[] array, int rangeStartIndex, int rangeLength, int numberOfShifts)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, array.Length, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(numberOfShifts, nameof(numberOfShifts));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex - numberOfShifts, nameof(numberOfShifts));

      ShiftRangeLeftInternal(in array, rangeStartIndex, rangeLength, numberOfShifts);
    }

    internal static void ShiftRangeLeftInternal<TItem>(in TItem[] array, int rangeStartIndex, int rangeLength, int numberOfShifts)
    {
      if (numberOfShifts == 0 || array.IsEmpty())
      {
        return;
      }

      int newIndex = rangeStartIndex - numberOfShifts;
      Array.Copy(array, rangeStartIndex, array, newIndex, rangeLength);
    }

    public static void ShiftRangeRight<TItem>(in TItem[] array, int rangeStartIndex, int rangeLength, int numberOfShifts)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, array.Length, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(numberOfShifts, nameof(numberOfShifts));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex + numberOfShifts, array.Length, nameof(numberOfShifts));

      ShiftRangeRightInternal(in array, rangeStartIndex, rangeLength, numberOfShifts);
    }

    internal static void ShiftRangeRightInternal<TItem>(in TItem[] array, int rangeStartIndex, int rangeLength, int numberOfShifts)
    {
      if (numberOfShifts == 0 || array.IsEmpty())
      {
        return;
      }

      int newIndex = rangeStartIndex + numberOfShifts;
      Array.Copy(array, rangeStartIndex, array, newIndex, rangeLength);
    }

#if !(NETSTANDARD2_0 || NETFRAMEWORK)
    public static void MoveRange<TItem>(ref TItem[] array, Range range, int newIndex)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));

      (int rangeStartIndex, int rangeLength) = range.GetOffsetAndLength(array.Length);
      int rangeEndIndex = rangeStartIndex + rangeLength;
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, array.Length, nameof(range.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeEndIndex, array.Length, nameof(range.End));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(newIndex, array.Length, nameof(newIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(newIndex + rangeLength, array.Length, nameof(newIndex));

      MoveInternal(ref array, rangeStartIndex, rangeLength, newIndex, isResizeEnabled: false);
    }

    public static void MoveRange<TItem>(ref TItem[] array, Range range, int newIndex, bool isMoveOutOfBoundsAllowed)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));

      (int rangeStartIndex, int rangeLength) = range.GetOffsetAndLength(array.Length);
      int rangeEndIndex = rangeStartIndex + rangeLength;
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, array.Length, nameof(range.Start));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeEndIndex, array.Length, nameof(range.End));

      if (!isMoveOutOfBoundsAllowed)
      {
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(newIndex, array.Length, nameof(newIndex));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(newIndex + rangeLength, array.Length, nameof(newIndex));
      }

      MoveInternal(ref array, rangeStartIndex, rangeLength, newIndex, isMoveOutOfBoundsAllowed);
    }
#endif

    public static void MoveRange<TItem>(ref TItem[] array, int rangeStartIndex, int rangeLength, int newIndex)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));

      int rangeEndIndex = rangeStartIndex + rangeLength;
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, array.Length, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeEndIndex, array.Length, nameof(rangeLength));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(newIndex, array.Length, nameof(newIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(newIndex + rangeLength, array.Length, nameof(newIndex));

      MoveInternal(ref array, rangeStartIndex, rangeLength, newIndex, isResizeEnabled: false);
    }

    public static void MoveRange<TItem>(ref TItem[] array, int rangeStartIndex, int rangeLength, int newIndex, bool isMoveOutOfBoundsAllowed)
    {
      ArgumentNullExceptionEx.ThrowIfNull(array, nameof(array));

      int rangeEndIndex = rangeStartIndex + rangeLength;
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, array.Length, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeEndIndex, array.Length, nameof(rangeLength));

      if (!isMoveOutOfBoundsAllowed)
      {
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(newIndex, array.Length, nameof(newIndex));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(newIndex + rangeLength, array.Length, nameof(newIndex));
      }

      MoveInternal(ref array, rangeStartIndex, rangeLength, newIndex, isMoveOutOfBoundsAllowed);
    }

    public static void Insert<TItem>(ref TItem[] destination, int index, TItem item)
    {
      ArgumentNullExceptionEx.ThrowIfNull(destination, nameof(destination));
      ArgumentNullExceptionEx.ThrowIfNull(item, nameof(item));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(index, destination.Length, nameof(index));

      int originalDestinationLength = destination.Length;
      int desiredSize = destination.Length + 1;
      Array.Resize(ref destination, desiredSize);

      bool isAddOperation = index == destination.Length;
      if (!isAddOperation)
      {
        int shiftRangeLength = originalDestinationLength - index;
        ArrayEx.ShiftRangeRight(in destination, index, shiftRangeLength, 1);
      }

      destination[index] = item;
    }

    public static void Insert<TItem>(ref TItem[] destination, int index, TItem[] source)
    {
      ArgumentNullExceptionEx.ThrowIfNull(destination, nameof(destination));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(index, destination.Length, nameof(index));

      InsertInternal(ref destination, index, source, 0, source.Length);
    }

    public static void Insert<TItem>(ref TItem[] destination, int index, TItem[] source, int sourceStartIndex, int sourceCount)
    {
      ArgumentNullExceptionEx.ThrowIfNull(destination, nameof(destination));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(index, destination.Length, nameof(index));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceStartIndex, nameof(sourceStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(sourceStartIndex, source.Length, nameof(sourceStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(sourceCount, nameof(sourceCount));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(sourceCount, source.Length - sourceStartIndex, nameof(sourceCount));

      InsertInternal(ref destination, index, source, sourceStartIndex, sourceCount);
    }

    public static void Insert<TItem>(ref TItem[] destination, int destinationStartIndex, IEnumerable<TItem> source, int rangeStartIndex, int rangeLength)
    {
      ArgumentNullExceptionEx.ThrowIfNull(destination, nameof(destination));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationStartIndex, nameof(destinationStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(destinationStartIndex, destination.Length, nameof(destinationStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(rangeStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(rangeLength));

      InsertInternal(ref destination, destinationStartIndex, source, rangeStartIndex, rangeLength);
    }

#if !(NETSTANDARD2_0 || NETFRAMEWORK)
    public static void Insert<TItem>(ref TItem[] destination, int destinationStartIndex, IEnumerable<TItem> source, Range sourceRange)
    {
      ArgumentNullExceptionEx.ThrowIfNull(destination, nameof(destination));
      ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
      ArgumentOutOfRangeExceptionEx.ThrowIfNegative(destinationStartIndex, nameof(destinationStartIndex));
      ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(destinationStartIndex, destination.Length, nameof(destinationStartIndex));

      if (source is TItem[] array)
      {
        (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(array.Length);
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, array.Length, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, destination.Length, nameof(sourceRange));
        InsertInternal(ref destination, destinationStartIndex, array, rangeStartIndex, rangeLength);
      }
      else if (source is IList<TItem> list)
      {
        (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(list.Count);
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, list.Count, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
        ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, destination.Length, nameof(sourceRange));
        InsertInternal(ref destination, destinationStartIndex, list, rangeStartIndex, rangeLength);
      }

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

      int takeCount = sourceRange.End.Value == 0 ? -1 : sourceRange.End.Value - sourceRange.Start.Value;
      InsertInternal(ref destination, destinationStartIndex, source, sourceRange.Start.Value, takeCount);
    }
#endif

    internal static void InsertInternal<TItem>(ref TItem[] destination, int index, TItem[] source, int sourceStartIndex, int sourceCount)
    {
      if (destination.IsEmpty())
      {
        destination = source;
      }
      else
      {
        int originalDestinationLength = destination.Length;
        int desiredLength = destination.Length + sourceCount;
        Array.Resize(ref destination, desiredLength);
        bool isAddRange = index == destination.Length;
        if (!isAddRange)
        {
          int numberOfShifts = sourceCount;
          int shiftRangeLength = originalDestinationLength - index;
          ArrayEx.ShiftRangeRightInternal(in destination, index, shiftRangeLength, numberOfShifts);
        }

        Array.Copy(source, sourceStartIndex, destination, index, source.Length);
      }
    }

    internal static void InsertInternal<TItem>(ref TItem[] destination, int destinationStartIndex, IList<TItem> source, int sourceStartIndex, int sourceCount)
    {
      if (source.IsEmpty())
      {
        return;
      }

      int sourceIndex = sourceStartIndex;
      bool isCopyFullSource = source.Count == sourceCount;
      if (destination.IsEmpty())
      {
        if (isCopyFullSource)
        {
          destination = source.ToArray();
        }
        else if (source is List<TItem> list)
        {
          Array.Resize(ref destination, sourceCount);
          list.CopyTo(sourceStartIndex, destination, 0, sourceCount);
        }
        else
        {
          Array.Resize(ref destination, sourceCount);
          for (int destinationIndex = 0; destinationIndex < destination.Length; destinationIndex++, sourceIndex++)
          {
            destination[destinationIndex] = source[sourceIndex];
          }
        }
      }
      else
      {
        int originalDestinationLength = destination.Length;
        int newLength = destination.Length + sourceCount;
        Array.Resize(ref destination, newLength);

        bool isAddRange = destinationStartIndex == destination.Length;
        if (!isAddRange)
        {
          int numberOfShifts = sourceCount;
          int shiftRangeLength = originalDestinationLength - destinationStartIndex;
          ArrayEx.ShiftRangeRightInternal(in destination, destinationStartIndex, shiftRangeLength, numberOfShifts);
        }

        if (source is List<TItem> list)
        {
          list.CopyTo(sourceStartIndex, destination, destinationStartIndex, sourceCount);
        }
        else
        {
          for (int destinationIndex = destinationStartIndex; destinationIndex < destination.Length; destinationIndex++, sourceIndex++)
          {
            destination[destinationIndex] = source[sourceIndex];
          }
        }
      }
    }

    internal static void InsertInternal<TItem>(ref TItem[] destination, int destinationStartIndex, IEnumerable<TItem> source, int rangeStartIndex, int rangeLength)
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
        int originalDestinationLength = destination.Length;
        int newSize = destination.Length + takeCount;
        Array.Resize(ref destination, newSize);

        if (!isAddRange)
        {
        }
        else
        {
          int shiftRangeLength = originalDestinationLength - destinationStartIndex;
          int numberOfShifts = rangeLength;
          ArrayEx.ShiftRangeRightInternal(in destination, destinationStartIndex, shiftRangeLength, numberOfShifts);
        }

        while ((takeCount > 0 || takeCount < 0) && sourceEnumerator.MoveNext())
        {
          destination[destinationIndex++] = sourceEnumerator.Current;
          takeCount--;
        }

        if (takeCount > 0)
        {
          destination = backup;
          throw new ArgumentOutOfRangeException(nameof(rangeLength));
        }
      }
    }

    internal static void MoveInternal<TItem>(ref TItem[] array, int rangeStartIndex, int rangeLength, int newIndex, bool isResizeEnabled)
    {
      if (newIndex == rangeStartIndex || rangeLength == 0 || array.IsEmpty())
      {
        return;
      }

      if (isResizeEnabled && newIndex + rangeLength > array.Length)
      {
        Array.Resize(ref array, newIndex + rangeLength);
      }

      int rangeEndIndex = rangeStartIndex + rangeLength;
      int numberOfShifts = System.Math.Abs(newIndex - rangeStartIndex);
      TItem[] items = new TItem[rangeLength];
      Array.Copy(array, rangeStartIndex, items, 0, rangeLength);
      if (newIndex > rangeStartIndex)
      {
        ShiftTrailingElementsLeft(array, rangeEndIndex, numberOfShifts, newIndex);
      }
      else if (newIndex < rangeStartIndex)
      {
        ShiftPrecedingElementsRight(array, rangeStartIndex - numberOfShifts, numberOfShifts, newIndex);
      }

      Array.Copy(items, 0, array, newIndex, rangeLength);
    }

    private static void ShiftTrailingElementsLeft<TItem>(in TItem[] array, int rangeStartIndex, int numberOfShifts, int newIndex)
    {
      int trailingElementsCount = numberOfShifts;
      ShiftRangeLeftInternal(in array, rangeStartIndex, trailingElementsCount, numberOfShifts);
    }

    private static void ShiftPrecedingElementsRight<TItem>(TItem[] array, int rangeStartIndex, int numberOfShifts, int newIndex)
    {
      int precedingElementsCount = numberOfShifts;
      ShiftRangeRightInternal(in array, rangeStartIndex, precedingElementsCount, numberOfShifts);
    }
  }
}
