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
    public List<int> EmptyList { get; }
    public ICollection<int> CollectionWith5Items0To4 { get; }
    public ICollection<int> NullReferenceCollection { get; }
    public List<int> ListWith5Items5To9 { get; }
    public List<int> ConcatenatedListResultOfOrderedAddRange { get; }
    public IDictionary<int, int> DictionaryWith5Items0To4 { get; }
    public Dictionary<int, int> DictionaryWith5Items5To9 { get; }
    public Dictionary<int, int> ConcatenatedDictionaryResultOfOrderedAddRange { get; }
    public Dictionary<int, int> NullReferenceDictionary { get; }

    public CollectionHelperExtensionsTestsAddRange() 
    {
      this.EmptyList = new List<int>();
      this.CollectionWith5Items0To4 = Enumerable.Range(0, 5).ToList();
      this.ListWith5Items5To9 = Enumerable.Range(5, 5).ToList();
      this.ConcatenatedListResultOfOrderedAddRange = new List<int>(this.CollectionWith5Items0To4.Concat(this.ListWith5Items5To9));

      var dictionarySeed = this.CollectionWith5Items0To4.ToDictionary(value => value);
      this.DictionaryWith5Items0To4 = new Dictionary<int, int>(dictionarySeed);

      dictionarySeed = this.ListWith5Items5To9.ToDictionary(value => value);
      this.DictionaryWith5Items5To9 = new Dictionary<int, int>(dictionarySeed);
      this.ConcatenatedDictionaryResultOfOrderedAddRange = this.DictionaryWith5Items0To4.Concat(this.DictionaryWith5Items5To9).ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    #region AddRange ICollection

    [Fact]
    public void AddList_ToListAddRange_MustAppendItemsOrdered()
    {
      _ = this.CollectionWith5Items0To4.AddRange(this.ListWith5Items5To9);

      _ = this.CollectionWith5Items0To4.Should().ContainInConsecutiveOrder(this.ConcatenatedListResultOfOrderedAddRange);
    }

    [Fact]
    public void AddList_ToList_MustReturnSameInstance()
    {
      IEnumerable<int> result = this.CollectionWith5Items0To4.AddRange(this.ListWith5Items5To9);

      _ = result.Should().BeSameAs(this.CollectionWith5Items0To4);
    }

    [Fact]
    public void CallICollectionExtensionMethodDirectly_PassingNull_MustThrow()
    {
      Action action = () => HelperExtensionsCommon.AddRange(this.NullReferenceCollection, this.ListWith5Items5To9);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void CallICollectionExtensionMethod_AddNull_MustThrow()
    {
      Action action = () => this.CollectionWith5Items0To4.AddRange(this.NullReferenceCollection);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    #endregion AddRange ICollection

    #region AddRange IDictioanary

    [Fact]
    public void AddIDictionary_ToDictionary_MustAppendItemsOrdered()
    {
      _ = this.DictionaryWith5Items0To4.AddRange(this.DictionaryWith5Items0To4);

      _ = this.DictionaryWith5Items0To4.Should().ContainInConsecutiveOrder(this.ConcatenatedDictionaryResultOfOrderedAddRange);
    }

    [Fact]
    public void AddIDictionary_ToListAddRange_MustReturnSameInstance()
    {
      IDictionary<int, int> result = this.DictionaryWith5Items0To4.AddRange(this.DictionaryWith5Items5To9);

      _ = result.Should().BeSameAs(this.DictionaryWith5Items0To4);
    }

    [Fact]
    public void CallIDictionaryExtensionMethodDirectly_PassingNull_MustThrow()
    {
      Action action = () => HelperExtensionsCommon.AddRange(this.NullReferenceDictionary, this.DictionaryWith5Items5To9);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void CallIDictionaryExtensionMethod_AddNull_MustThrow()
    {
      Action action = () => this.DictionaryWith5Items0To4.AddRange(this.NullReferenceDictionary);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    #endregion AddRange IDictioanary

    #region AddRange IEnumerable<KeyValuePair>

    [Fact]
    public void AddIEnumerableKeyValuePair_ToDictionary_MustAppendItemsOrdered()
    {
      IEnumerable<KeyValuePair<int, int>> range = this.DictionaryWith5Items5To9.ToList();
      _ = this.DictionaryWith5Items0To4.AddRange(range);

      _ = this.DictionaryWith5Items0To4.Should().ContainInConsecutiveOrder(this.ConcatenatedDictionaryResultOfOrderedAddRange);
    }

    [Fact]
    public void AddIEnumerableKeyValuePair_ToListAddRange_MustReturnSameInstance()
    {
      IEnumerable<KeyValuePair<int, int>> range = this.DictionaryWith5Items5To9.ToList();
      IDictionary<int, int> result = this.DictionaryWith5Items0To4.AddRange(range);

      _ = result.Should().BeSameAs(this.DictionaryWith5Items0To4);
    }

    [Fact]
    public void CallIEnumerableKeyValuePairExtensionMethodDirectly_PassingNull_MustThrow()
    {
      IEnumerable<KeyValuePair<int, int>> range = this.DictionaryWith5Items5To9.ToList();
      Action action = () => HelperExtensionsCommon.AddRange(this.NullReferenceDictionary, range);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void CallIEnumerableKeyValuePairExtensionMethod_AddNull_MustThrow()
    {
      IEnumerable<KeyValuePair<int, int>> range = null;
      Action action = () => this.DictionaryWith5Items0To4.AddRange(range);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    #endregion AddRange IEnumerable<KeyValuePair>

    #region AddRange IEnumerableTuple

    [Fact]
    public void AddIEnumerableTuple_ToDictionary_MustAppendItemsOrdered()
    {
      IEnumerable<(int Key, int Value)> range = this.DictionaryWith5Items5To9.Select(entry => (entry.Key, entry.Value));
      _ = this.DictionaryWith5Items0To4.AddRange(range);

      _ = this.DictionaryWith5Items0To4.Should().ContainInConsecutiveOrder(this.ConcatenatedDictionaryResultOfOrderedAddRange);
    }

    [Fact]
    public void AddIEnumerableTuple_ToListAddRange_MustReturnSameInstance()
    {
      IEnumerable<(int Key, int Value)> range = this.DictionaryWith5Items5To9.Select(entry => (entry.Key, entry.Value));
      IDictionary<int, int> result = this.DictionaryWith5Items0To4.AddRange(range);

      _ = result.Should().BeSameAs(this.DictionaryWith5Items0To4);
    }

    [Fact]
    public void CallIEnumerableTupleExtensionMethodDirectly_PassingNull_MustThrow()
    {
      IEnumerable<(int Key, int Value)> range = this.DictionaryWith5Items5To9.Select(entry => (entry.Key, entry.Value));
      Action action = () => HelperExtensionsCommon.AddRange(this.NullReferenceDictionary, range);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void CallIEnumerableTupleExtensionMethod_AddNull_MustThrow()
    {
      IEnumerable<(int Key, int Value)> range = null;
      Action action = () => this.DictionaryWith5Items0To4.AddRange(range);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }

    #endregion AddRange IEnumerable<KeyValuePair>
  }
}
