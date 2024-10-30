namespace BionicCode.Utilities.Net.UnitTest.ExtensionMethodsTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using FluentAssertions;
  using Xunit;
  using BionicCode.Utilities.Net;
  using Microsoft.CodeAnalysis.CSharp.Syntax;

  public class CollectionHelperExtensionsTestsInsertRange
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

    public CollectionHelperExtensionsTestsInsertRange() 
    {
      IEnumerable<int> firstRange = Enumerable.Range(FirstRangeStart, RangeCount);
      this.CollectionWithFirstRange = firstRange.ToList();
      IEnumerable<int> secondRange = Enumerable.Range(SecondRangeStart, RangeCount);

      this.EmptyList = new List<int>();
      this.NullReferenceCollection = null;
      this.ListWithSecondRange = secondRange.ToList();
      this.ConcatenatedListResultOfOrderedAddRange = firstRange.Concat(secondRange).ToList();

      this.NullReferenceDictionary = null;
      Dictionary<int, int> firstRangeDictionarySeed = firstRange.ToDictionary(value => value);
      this.CollectionWithKeyValuePairItemsFirstRange = new List<KeyValuePair<int, int>>(firstRangeDictionarySeed);
      this.DictionaryWithFirstRange = new Dictionary<int, int>(firstRangeDictionarySeed);

      Dictionary<int, int> secondRangeDictionarySeed = secondRange.ToDictionary(value => value);
      this.DictionaryWithSecondRange = new Dictionary<int, int>(secondRangeDictionarySeed);
      this.ConcatenatedDictionaryResultOfOrderedAddRange = firstRangeDictionarySeed.Concat(secondRangeDictionarySeed).ToDictionary(entry => entry.Key, entry => entry.Value);

      this.EmptyArray = Array.Empty<int>();
      this.NullReferenceArray = null;
      this.ArrayWithFirstRange = firstRange.ToArray();
      this.ArrayWithSecondRange = secondRange.ToArray();
      this.ConcatenatedArrayResultOfOrderedAddRange = firstRange.Concat(secondRange).ToArray();
    }

    #region InsertRange ICollection

    [Fact]
    public void AddList_ToCollectionAddRange_MustAppendItemsOrdered()
    {
      this.CollectionWithFirstRange.AddRange(this.ListWithSecondRange);

      _ = this.CollectionWithFirstRange.Should().ContainInConsecutiveOrder(this.ConcatenatedListResultOfOrderedAddRange);
    }

    [Fact]
    public void AddDictionary_ToKeyValuePairCollectionAddRange_MustAppendItemsOrdered()
    {
      this.CollectionWithKeyValuePairItemsFirstRange.AddRange(this.DictionaryWithSecondRange);

      _ = this.CollectionWithKeyValuePairItemsFirstRange.Should().ContainInConsecutiveOrder(this.ConcatenatedDictionaryResultOfOrderedAddRange);
    }

    [Fact]
    public void AddKeyValuePairsWithDuplicateKeys_ToCollection_MustNotThrow()
    {
      Action action = () => this.CollectionWithKeyValuePairItemsFirstRange.AddRange(this.DictionaryWithSecondRange);

      _ = action.Should().NotThrow("because duplicate keys are allowed in collections");
    }

    [Fact]
    public void CallICollectionExtensionMethodDirectly_PassingNull_MustThrow()
    {
      Action action = () => HelperExtensionsCommon.AddRange(this.NullReferenceCollection, this.ListWithSecondRange);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void CallICollectionExtensionMethod_AddNull_MustThrow()
    {
      Action action = () => this.CollectionWithFirstRange.AddRange(this.NullReferenceCollection);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    #endregion InsertRange ICollection

    #region InsertRange Array

    [Fact]
    public void AddArray_ToArrayAddRange_MustReturnEnlargedArray()
    {
      int[] result = this.ArrayWithFirstRange.AddRange(this.ArrayWithSecondRange);

      _ = result.Length.Should().BeGreaterThan(this.ArrayWithFirstRange.Length);
    }

    [Fact]
    public void AddArray_ToArrayAddRange_MustAppendItemsOrdered()
    {
      int[] result = this.ArrayWithFirstRange.AddRange(this.ArrayWithSecondRange);

      _ = result.Should().ContainInConsecutiveOrder(this.ConcatenatedArrayResultOfOrderedAddRange);
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(1, -1, 0)]
    [InlineData(1, 0, -1)]
    [InlineData(RangeCount + 1, 0, 0)]
    [InlineData(1, RangeCount, 0)]
    [InlineData(1, 0, RangeCount + 1)]
    [InlineData(1, RangeCount - 1, 2)]
    public void InsertArrayInvalidRange_ToArray_MustThrow(int destinationStartIndex, int sourceStartIndex, int sourceCount)
    {
      Action action = () => this.ArrayWithFirstRange.InsertRange(destinationStartIndex, this.ArrayWithSecondRange, sourceStartIndex, sourceCount);

      _ = action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddIEnumerable_ToArrayAddRange_MustAppendItemsOrdered()
    {
      IEnumerable<int> range = this.ArrayWithSecondRange.Select(item => item);

      int[] result = this.ArrayWithFirstRange.AddRange(range);

      _ = result.Should().ContainInConsecutiveOrder(this.ConcatenatedArrayResultOfOrderedAddRange);
    }

    [Fact]
    public void AddList_ToArrayAddRange_MustAppendItemsOrdered()
    {
      List<int> range = this.ArrayWithSecondRange.ToList();

      int[] result = this.ArrayWithFirstRange.AddRange(range);

      _ = result.Should().ContainInConsecutiveOrder(this.ConcatenatedArrayResultOfOrderedAddRange);
    }

    [Fact]
    public void AddIEnumerable_ToEmptyArrayAddRange_MustReturnNewArray()
    {
      IEnumerable<int> range = this.ArrayWithSecondRange.Select(item => item);

      int[] result = this.EmptyArray.AddRange(range);

      _ = result.Should().ContainInConsecutiveOrder(this.ArrayWithSecondRange);
    }

    [Fact]
    public void AddList_ToEmptyArrayAddRange_MustReturnNewArray()
    {
      List<int> range = this.ArrayWithSecondRange.ToList();

      int[] result = this.EmptyArray.AddRange(range);

      _ = result.Should().ContainInConsecutiveOrder(this.ArrayWithSecondRange);
    }

    [Fact]
    public void AddArray_ToEmptyArrayAddRange_MustReturnAddedArray()
    {
      int[] result = this.EmptyArray.AddRange(this.ArrayWithSecondRange);

      _ = result.Should().BeSameAs(this.ArrayWithSecondRange);
    }

    [Fact]
    public void AddEmptyArray_ToArrayAddRange_MustReturnOriginalArray()
    {
      int[] result = this.ArrayWithFirstRange.AddRange(this.EmptyArray);

      _ = result.Should().BeSameAs(this.ArrayWithFirstRange);
    }

    [Fact]
    public void AddEmptyList_ToArrayAddRange_MustReturnOriginalArray()
    {
      int[] result = this.ArrayWithFirstRange.AddRange(this.EmptyList);

      _ = result.Should().BeSameAs(this.ArrayWithFirstRange);
    }

    [Fact]
    public void AddEmptyIEnumerable_ToArrayAddRange_MustReturnOriginalArray()
    {
      int[] result = this.ArrayWithFirstRange.AddRange(Enumerable.Empty<int>());

      _ = result.Should().BeSameAs(this.ArrayWithFirstRange);
    }

    [Fact]
    public void CallIEnumerableKeyValuePairExtensionMethodDirectly_PassingNull_MustThrow()
    {
      IEnumerable<KeyValuePair<int, int>> range = this.DictionaryWithSecondRange.ToList();
      Action action = () => HelperExtensionsCommon.AddRange(this.NullReferenceDictionary, range);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void CallIEnumerableKeyValuePairExtensionMethod_AddNull_MustThrow()
    {
      IEnumerable<KeyValuePair<int, int>> range = null;
      Action action = () => this.DictionaryWithFirstRange.AddRange(range);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    #endregion InsertRange Array
  }
}
