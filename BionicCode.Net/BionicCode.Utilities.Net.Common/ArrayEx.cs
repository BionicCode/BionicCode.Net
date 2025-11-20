namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Provides static utility methods for manipulating and modifying arrays, including moving, shifting, and inserting
    /// elements or ranges within arrays.
    /// </summary>
    /// <remarks>The methods in this class extend the functionality of standard arrays by enabling advanced
    /// operations such as moving single elements or ranges to new positions, shifting ranges left or right, and inserting
    /// items or collections at specified indices. These operations are performed in-place and may resize the array as
    /// needed. All methods validate input parameters and throw exceptions for invalid arguments. This class is
    /// thread-unsafe; concurrent modifications to the same array instance should be synchronized externally.</remarks>
    public static class ArrayEx
    {
        private const int DynamicGrowCapacity = 10;

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
                int rangeLength = oldIndex - newIndex;
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
            Debug.Assert(rangeLength > -1);
            Debug.Assert(rangeLength < array.Length);
            Debug.Assert(numberOfShifts <= array.Length - rangeLength - rangeStartIndex);

            if (numberOfShifts == 0
              || rangeLength == 0
              || array.IsEmpty())
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
            Debug.Assert(rangeLength > -1);
            Debug.Assert(rangeLength < array.Length);
            Debug.Assert(numberOfShifts <= array.Length - rangeLength - rangeStartIndex);

            if (numberOfShifts == 0
               || rangeLength == 0
               || array.IsEmpty())
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
        public static void Insert<TItem>(ref TItem[] destination, int index, IEnumerable<TItem> source, Range sourceRange)
        {
            ArgumentNullExceptionEx.ThrowIfNull(destination, nameof(destination));
            ArgumentNullExceptionEx.ThrowIfNull(source, nameof(source));
            ArgumentOutOfRangeExceptionEx.ThrowIfNegative(index, nameof(index));
            ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(index, destination.Length, nameof(index));

            if (source is TItem[] array)
            {
                (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(array.Length);
                ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
                ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, array.Length, nameof(sourceRange));
                ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
                ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, destination.Length, nameof(sourceRange));
                InsertInternal(ref destination, index, array, rangeStartIndex, rangeLength);
            }
            else if (source is IList<TItem> list)
            {
                (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(list.Count);
                ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
                ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, list.Count, nameof(sourceRange));
                ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
                ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, destination.Length, nameof(sourceRange));
                InsertInternal(ref destination, index, list, rangeStartIndex, rangeLength);
            }
            else if (source is ICollection<TItem> genericCollection)
            {
                (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(genericCollection.Count);
                ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
                ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, genericCollection.Count, nameof(sourceRange));
                ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
                ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, destination.Length, nameof(sourceRange));

                ArrayEx.InsertInternal(ref destination, destination.Length, source, rangeStartIndex, rangeLength);
            }
            else if (source is ICollection collection)
            {
                (int rangeStartIndex, int rangeLength) = sourceRange.GetOffsetAndLength(collection.Count);
                ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeStartIndex, nameof(sourceRange));
                ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThanOrEqual(rangeStartIndex, collection.Count, nameof(sourceRange));
                ArgumentOutOfRangeExceptionEx.ThrowIfNegative(rangeLength, nameof(sourceRange));
                ArgumentOutOfRangeExceptionEx.ThrowIfGreaterThan(rangeLength, destination.Length, nameof(sourceRange));

                ArrayEx.InsertInternal(ref destination, destination.Length, source, rangeStartIndex, rangeLength);
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

                ArrayEx.InsertInternal(ref destination, destination.Length, source, sourceRange.Start.Value, takeCount);
            }
        }
#endif

        internal static void InsertInternal<TItem>(ref TItem[] destination, int index, TItem[] source, int sourceStartIndex, int sourceCount)
        {
            Debug.Assert(sourceCount > -1);
            Debug.Assert(sourceCount <= source.Length);
            Debug.Assert(index > -1);
            Debug.Assert(index <= destination.Length);
            Debug.Assert(sourceStartIndex > -1);
            Debug.Assert(sourceStartIndex < source.Length || sourceStartIndex == 0);

            if (source.IsEmpty() || sourceCount == 0)
            {
                return;
            }

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
            Debug.Assert(sourceCount > -1);
            Debug.Assert(sourceCount <= source.Count);
            Debug.Assert(destinationStartIndex > -1);
            Debug.Assert(destinationStartIndex <= destination.Length);
            Debug.Assert(sourceStartIndex > -1);
            Debug.Assert(sourceStartIndex < source.Count || sourceStartIndex == 0);

            if (source.IsEmpty() || sourceCount == 0)
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
            Debug.Assert(destinationStartIndex > -1);
            Debug.Assert(destinationStartIndex <= destination.Length);
            Debug.Assert(rangeLength >= -1);

            if (rangeLength == 0 || !source.Any())
            {
                return;
            }

            bool requiresDynamicAllocation = rangeLength == -1;
            int skipCount = rangeStartIndex;
            int takeCount = rangeLength;
            using IEnumerator<TItem> sourceEnumerator = source.GetEnumerator();

            while (skipCount > 0 && sourceEnumerator.MoveNext())
            {
                skipCount--;
            }

            if (skipCount > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rangeStartIndex));
            }

            bool isAddRange = destination.Length == destinationStartIndex;
            int originalDestinationLength = destination.Length;
            TItem[] backup = new TItem[originalDestinationLength];
            destination.CopyTo(backup, 0);
            int newSize = requiresDynamicAllocation
              ? System.Math.Max(DynamicGrowCapacity, destination.Length * 2)
              : destination.Length + takeCount;

            if (newSize > Array.MaxLength)
            {
                newSize = Array.MaxLength;
            }

            Array.Resize(ref destination, newSize);

            if (!isAddRange && !requiresDynamicAllocation)
            {
                int shiftRangeLength = originalDestinationLength - destinationStartIndex;
                int numberOfShifts = rangeLength;
                ArrayEx.ShiftRangeRightInternal(in destination, destinationStartIndex, shiftRangeLength, numberOfShifts);
            }

            int currentIndex = destinationStartIndex;
            int addedItemCount = 0;
            while ((takeCount > 0 || takeCount < 0) && sourceEnumerator.MoveNext())
            {
                if (currentIndex >= destination.Length)
                {
                    newSize = destination.Length * 2;

                    if (newSize > Array.MaxLength)
                    {
                        newSize = Array.MaxLength;
                    }

                    Array.Resize(ref destination, newSize);
                }

                destination[currentIndex++] = sourceEnumerator.Current;
                takeCount--;
                addedItemCount++;
            }

            if (takeCount > 0)
            {
                destination = backup;
                throw new ArgumentOutOfRangeException(nameof(rangeLength));
            }

            if (requiresDynamicAllocation)
            {
                int availableSize = destination.Length - currentIndex;
                int desiredSize = originalDestinationLength - destinationStartIndex;
                if (desiredSize > availableSize)
                {
                    int growthRequired = desiredSize - availableSize;
                    Array.Resize(ref destination, destination.Length + growthRequired);
                }

                Array.Copy(backup, destinationStartIndex, destination, currentIndex, originalDestinationLength - destinationStartIndex);
                bool hasTrailingUnusedMemory = availableSize > desiredSize;
                if (hasTrailingUnusedMemory)
                {
                    int sizeToTrim = availableSize - desiredSize;
                    Array.Resize(ref destination, destination.Length - sizeToTrim);
                }
            }

            Debug.Assert(destination.Length == originalDestinationLength + addedItemCount);
        }

        internal static void MoveInternal<TItem>(ref TItem[] array, int rangeStartIndex, int rangeLength, int newIndex, bool isResizeEnabled)
        {
            Debug.Assert(rangeStartIndex > -1);
            Debug.Assert(rangeStartIndex < array.Length);
            Debug.Assert(rangeLength >= 0);
            Debug.Assert(rangeLength <= array.Length);
            Debug.Assert(newIndex > -1);
            Debug.Assert(newIndex < array.Length);

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
