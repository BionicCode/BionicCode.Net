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

    #region AddRange ICollection

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

    #endregion AddRange ICollection

    #region AddRange IDictioanary

    [Fact]
    public void AddIDictionary_ToDictionary_MustAppendItemsOrdered()
    {
      this.DictionaryWithFirstRange.AddRange(this.DictionaryWithSecondRange);

      _ = this.DictionaryWithFirstRange.Should().ContainInConsecutiveOrder(this.ConcatenatedDictionaryResultOfOrderedAddRange);
    }

    [Fact]
    public void AddIEnumerableKeyValuePair_ToDictionary_MustAppendItemsOrdered()
    {
      IEnumerable<KeyValuePair<int, int>> range = this.DictionaryWithSecondRange.ToList();

      this.DictionaryWithFirstRange.AddRange(range);

      _ = this.DictionaryWithFirstRange.Should().ContainInConsecutiveOrder(this.ConcatenatedDictionaryResultOfOrderedAddRange);
    }

    [Fact]
    public void AddIEnumerableTuple_ToDictionary_MustAppendItemsOrdered()
    {
      IEnumerable<(int Key, int Value)> range = this.DictionaryWithSecondRange.Select(entry => (entry.Key, entry.Value));
      
      this.DictionaryWithFirstRange.AddRange(range);

      _ = this.DictionaryWithFirstRange.Should().ContainInConsecutiveOrder(this.ConcatenatedDictionaryResultOfOrderedAddRange);
    }

    [Fact]
    public void AddIDictionary_WithDuplicateKeysToDictionary_MustThrow()
    {
      Action action = () => this.DictionaryWithFirstRange.AddRange(this.DictionaryWithFirstRange);

      _ = action.Should().Throw<ArgumentException>("because of duplicate keys");
    }

    [Fact]
    public void CallIDictionaryExtensionMethodDirectly_PassingNull_MustThrow()
    {
      Action action = () => HelperExtensionsCommon.AddRange(this.NullReferenceDictionary, this.DictionaryWithSecondRange);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void CallIDictionaryExtensionMethod_AddNull_MustThrow()
    {
      Action action = () => this.DictionaryWithFirstRange.AddRange(this.NullReferenceDictionary);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    #endregion AddRange IDictioanary

    #region AddRange Array

    [Fact]
    public void AddArray_ToArrayAddRange_MustReturnEnlargedArray()
    {
      int[] result = this.ArrayWithFirstRange.AddRange(this.ArrayWithSecondRange);

      _ = result.Length.Should().BeGreaterThan(this.ArrayWithFirstRange.Length);
    }

    [Fact]
    public void AddIEnumerable_ToArrayAddRange_MustAppendItemsOrdered()
    {
      IEnumerable<KeyValuePair<int, int>> range = this.DictionaryWithSecondRange.ToList();
      IDictionary<int, int> result = this.DictionaryWithFirstRange.AddRange(range);

      _ = result.Should().BeSameAs(this.DictionaryWithFirstRange);
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

    #endregion AddRange Array
  }
}
