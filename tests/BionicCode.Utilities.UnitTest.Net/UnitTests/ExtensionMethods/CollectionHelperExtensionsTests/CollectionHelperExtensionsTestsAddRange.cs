namespace BionicCode.Utilities.Net.UnitTest.ExtensionMethodsTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BionicCode.Utilities.Net;
    using FluentAssertions;
    using Xunit;

    public class CollectionHelperExtensionsTestsAddRange
    {
        public const int RangeCount = 5;
        public const int FirstRangeStart = 0;
        public const int SecondRangeStart = RangeCount;

        public List<int> EmptyList { get; }
        public ICollection<int> NullReferenceCollection { get; }
        public ICollection<int> CollectionWithFirstRange { get; }
        public List<int> ListWithSecondRange { get; }
        public ICollection<KeyValuePair<int, int>> CollectionWithKeyValuePairItemsFirstRange { get; }
        public List<int> ConcatenatedListResultOfOrderedAddRange { get; }
        public int[] EmptyArray { get; }
        public int[] NullReferenceArray { get; }
        public int[] ArrayWithFirstRange { get; }
        public int[] ArrayWithSecondRange { get; }
        public int[] ConcatenatedArrayResultOfOrderedAddRange { get; }
        public IDictionary<int, int> DictionaryWithFirstRange { get; }
        public Dictionary<int, int> DictionaryWithSecondRange { get; }
        public Dictionary<int, int> ConcatenatedDictionaryResultOfOrderedAddRange { get; }
        public Dictionary<int, int> NullReferenceDictionary { get; }

        public CollectionHelperExtensionsTestsAddRange()
        {
            IEnumerable<int> firstRange = Enumerable.Range(FirstRangeStart, RangeCount);
            CollectionWithFirstRange = firstRange.ToList();
            IEnumerable<int> secondRange = Enumerable.Range(SecondRangeStart, RangeCount);

            EmptyList = new List<int>();
            NullReferenceCollection = null;
            ListWithSecondRange = secondRange.ToList();
            ConcatenatedListResultOfOrderedAddRange = firstRange.Concat(secondRange).ToList();

            NullReferenceDictionary = null;
            Dictionary<int, int> firstRangeDictionarySeed = firstRange.ToDictionary(value => value);
            CollectionWithKeyValuePairItemsFirstRange = new List<KeyValuePair<int, int>>(firstRangeDictionarySeed);
            DictionaryWithFirstRange = new Dictionary<int, int>(firstRangeDictionarySeed);

            Dictionary<int, int> secondRangeDictionarySeed = secondRange.ToDictionary(value => value);
            DictionaryWithSecondRange = new Dictionary<int, int>(secondRangeDictionarySeed);
            ConcatenatedDictionaryResultOfOrderedAddRange = firstRangeDictionarySeed.Concat(secondRangeDictionarySeed).ToDictionary(entry => entry.Key, entry => entry.Value);

            EmptyArray = Array.Empty<int>();
            NullReferenceArray = null;
            ArrayWithFirstRange = firstRange.ToArray();
            ArrayWithSecondRange = secondRange.ToArray();
            ConcatenatedArrayResultOfOrderedAddRange = firstRange.Concat(secondRange).ToArray();
        }

        #region AddRange ICollection

        [Fact]
        public void AddList_ToCollectionAddRange_MustAppendItemsOrdered()
        {
            CollectionWithFirstRange.AddRange(ListWithSecondRange);

            _ = CollectionWithFirstRange.Should().ContainInConsecutiveOrder(ConcatenatedListResultOfOrderedAddRange);
        }

        [Fact]
        public void AddDictionary_ToKeyValuePairCollectionAddRange_MustAppendItemsOrdered()
        {
            CollectionWithKeyValuePairItemsFirstRange.AddRange(DictionaryWithSecondRange);

            _ = CollectionWithKeyValuePairItemsFirstRange.Should().ContainInConsecutiveOrder(ConcatenatedDictionaryResultOfOrderedAddRange);
        }

        [Fact]
        public void AddKeyValuePairsWithDuplicateKeys_ToCollection_MustNotThrow()
        {
            Action action = () => CollectionWithKeyValuePairItemsFirstRange.AddRange(DictionaryWithSecondRange);

            _ = action.Should().NotThrow("because duplicate keys are allowed in collections");
        }

        [Fact]
        public void CallICollectionExtensionMethodDirectly_PassingNull_MustThrow()
        {
            Action action = () => HelperExtensionsCommon.AddRange(NullReferenceCollection, ListWithSecondRange);

            _ = action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void CallICollectionExtensionMethod_AddNull_MustThrow()
        {
            Action action = () => CollectionWithFirstRange.AddRange(NullReferenceCollection);

            _ = action.Should().ThrowExactly<ArgumentNullException>();
        }

        #endregion AddRange ICollection

        #region AddRange IDictioanary

        [Fact]
        public void AddIDictionary_ToDictionary_MustAppendItemsOrdered()
        {
            DictionaryWithFirstRange.AddRange(DictionaryWithSecondRange);

            _ = DictionaryWithFirstRange.Should().ContainInConsecutiveOrder(ConcatenatedDictionaryResultOfOrderedAddRange);
        }

        [Fact]
        public void AddIEnumerableKeyValuePair_ToDictionary_MustAppendItemsOrdered()
        {
            IEnumerable<KeyValuePair<int, int>> range = DictionaryWithSecondRange.ToList();

            DictionaryWithFirstRange.AddRange(range);

            _ = DictionaryWithFirstRange.Should().ContainInConsecutiveOrder(ConcatenatedDictionaryResultOfOrderedAddRange);
        }

        [Fact]
        public void AddIEnumerableTuple_ToDictionary_MustAppendItemsOrdered()
        {
            IEnumerable<(int Key, int Value)> range = DictionaryWithSecondRange.Select(entry => (entry.Key, entry.Value));

            DictionaryWithFirstRange.AddRange(range);

            _ = DictionaryWithFirstRange.Should().ContainInConsecutiveOrder(ConcatenatedDictionaryResultOfOrderedAddRange);
        }

        [Fact]
        public void AddIDictionary_WithDuplicateKeysToDictionary_MustThrow()
        {
            Action action = () => DictionaryWithFirstRange.AddRange(DictionaryWithFirstRange);

            _ = action.Should().Throw<ArgumentException>("because of duplicate keys");
        }

        [Fact]
        public void CallIDictionaryExtensionMethodDirectly_PassingNull_MustThrow()
        {
            Action action = () => HelperExtensionsCommon.AddRange(NullReferenceDictionary, DictionaryWithSecondRange);

            _ = action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void CallIDictionaryExtensionMethod_AddNull_MustThrow()
        {
            Action action = () => DictionaryWithFirstRange.AddRange(NullReferenceDictionary);

            _ = action.Should().ThrowExactly<ArgumentNullException>();
        }

        #endregion AddRange IDictioanary

        #region AddRange Array

        [Fact]
        public void AddArray_ToArrayAddRange_MustReturnEnlargedArray()
        {
            int[] result = ArrayWithFirstRange.AddRange(ArrayWithSecondRange);

            _ = result.Length.Should().BeGreaterThan(ArrayWithFirstRange.Length);
        }

        [Fact]
        public void AddArray_ToArrayAddRange_MustAppendItemsOrdered()
        {
            int[] result = ArrayWithFirstRange.AddRange(ArrayWithSecondRange);

            _ = result.Should().ContainInConsecutiveOrder(ConcatenatedArrayResultOfOrderedAddRange);
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(RangeCount, 0)]
        [InlineData(0, RangeCount + 1)]
        [InlineData(RangeCount - 1, 2)]
        public void AddArrayInvalidRange_ToArrayAddRange_MustThrow(int sourceStartIndex, int sourceCount)
        {
            Action action = () => ArrayWithFirstRange.AddRange(ArrayWithSecondRange, sourceStartIndex, sourceCount);

            _ = action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void AddIEnumerable_ToArrayAddRange_MustAppendItemsOrdered()
        {
            IEnumerable<int> range = ArrayWithSecondRange.Select(item => item);

            int[] result = ArrayWithFirstRange.AddRange(range);

            _ = result.Should().ContainInConsecutiveOrder(ConcatenatedArrayResultOfOrderedAddRange);
        }

        [Fact]
        public void AddList_ToArrayAddRange_MustAppendItemsOrdered()
        {
            List<int> range = ArrayWithSecondRange.ToList();

            int[] result = ArrayWithFirstRange.AddRange(range);

            _ = result.Should().ContainInConsecutiveOrder(ConcatenatedArrayResultOfOrderedAddRange);
        }

        [Fact]
        public void AddIEnumerable_ToEmptyArrayAddRange_MustReturnNewArray()
        {
            IEnumerable<int> range = ArrayWithSecondRange.Select(item => item);

            int[] result = EmptyArray.AddRange(range);

            _ = result.Should().ContainInConsecutiveOrder(ArrayWithSecondRange);
        }

        [Fact]
        public void AddList_ToEmptyArrayAddRange_MustReturnNewArray()
        {
            List<int> range = ArrayWithSecondRange.ToList();

            int[] result = EmptyArray.AddRange(range);

            _ = result.Should().ContainInConsecutiveOrder(ArrayWithSecondRange);
        }

        [Fact]
        public void AddArray_ToEmptyArrayAddRange_MustReturnAddedArray()
        {
            int[] result = EmptyArray.AddRange(ArrayWithSecondRange);

            _ = result.Should().BeSameAs(ArrayWithSecondRange);
        }

        [Fact]
        public void AddEmptyArray_ToArrayAddRange_MustReturnOriginalArray()
        {
            int[] result = ArrayWithFirstRange.AddRange(EmptyArray);

            _ = result.Should().BeSameAs(ArrayWithFirstRange);
        }

        [Fact]
        public void AddEmptyList_ToArrayAddRange_MustReturnOriginalArray()
        {
            int[] result = ArrayWithFirstRange.AddRange(EmptyList);

            _ = result.Should().BeSameAs(ArrayWithFirstRange);
        }

        [Fact]
        public void AddEmptyIEnumerable_ToArrayAddRange_MustReturnOriginalArray()
        {
            int[] result = ArrayWithFirstRange.AddRange(Enumerable.Empty<int>());

            _ = result.Should().BeSameAs(ArrayWithFirstRange);
        }

        [Fact]
        public void CallIEnumerableKeyValuePairExtensionMethodDirectly_PassingNull_MustThrow()
        {
            IEnumerable<KeyValuePair<int, int>> range = DictionaryWithSecondRange.ToList();
            Action action = () => HelperExtensionsCommon.AddRange(NullReferenceDictionary, range);

            _ = action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void CallIEnumerableKeyValuePairExtensionMethod_AddNull_MustThrow()
        {
            IEnumerable<KeyValuePair<int, int>> range = null;
            Action action = () => DictionaryWithFirstRange.AddRange(range);

            _ = action.Should().ThrowExactly<ArgumentNullException>();
        }

        #endregion AddRange Array
    }
}
