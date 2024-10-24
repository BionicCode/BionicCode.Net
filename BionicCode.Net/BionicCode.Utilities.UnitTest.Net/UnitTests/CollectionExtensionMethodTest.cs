namespace BionicCode.Utilities.Net.UnitTest
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using BionicCode.Utilities.Net;
  using FluentAssertions;
  using Xunit;

  public class TestContext
  {
    public int ItemsCount { get; }
    public int NewItemsCount { get; }
    public ICollection<int> Items { get; private set; }
    public ICollection<int> ItemsBackup { get; }
    public Dictionary<int, int> ItemTable { get; private set; }
    public Dictionary<int, int> ItemTableBackup { get; }
    public Dictionary<int, int> NewTableItemsFromDictionary { get; }
    public IList<(int Key, int Value)> NewTableItemsFromTupleCollection { get; }
    public IList<KeyValuePair<int, int>> NewTableItemsFromKeyValuePairCollection { get; }
    public ICollection<int> NewItems { get; }
    public ICollection<int> EmptyItems { get; }
    public Func<int, bool> FailingContainsPredicate => item => item > this.Items.Last();
    public Func<int, bool> SuceedingContainsPredicate => item => item < 5;

    public TestContext()
    {
      this.ItemsCount = 1000;
      this.NewItemsCount = 10;

      this.Items = new List<int>();
      this.ItemsBackup = new List<int>();
      this.NewItems = new List<int>();
      this.EmptyItems = new List<int>();

      this.ItemTable = new Dictionary<int, int>();
      this.ItemTableBackup = new Dictionary<int, int>();
      this.NewTableItemsFromDictionary = new Dictionary<int, int>();
      this.NewTableItemsFromTupleCollection = new List<(int, int)>();
      this.NewTableItemsFromKeyValuePairCollection = new List<KeyValuePair<int, int>>();

      for (int count = 0; count < this.ItemsCount; count++)
      {
        int key = count;
        int value = count * 10;
        this.ItemTable.Add(key, value);
        this.Items.Add(count);
        this.ItemTableBackup.Add(key, value);
        this.ItemsBackup.Add(count);
      }

      for (int count = this.ItemsCount; count < this.ItemsCount + this.NewItemsCount; count++)
      {
        int key = count;
        int value = count * 10;
        this.NewItems.Add(count);
        this.NewTableItemsFromTupleCollection.Add((key, value));
        this.NewTableItemsFromKeyValuePairCollection.Add(new KeyValuePair<int, int>(key, value));
        this.NewTableItemsFromDictionary.Add(key, value);
      }
    }

    public void Reset()
    {
      this.Items = new List<int>(this.ItemsBackup);
      this.ItemTable = new Dictionary<int, int>(this.ItemTableBackup);
    }
  }

  public class CollectionExtensionMethodTest : IClassFixture<TestContext>, IDisposable
  {
    private TestContext Context { get; }
    public CollectionExtensionMethodTest(TestContext context) => this.Context = context;

    [Fact]
    public void IsEmpty_ReturnsTrueForEmptyCollection() => this.Context.EmptyItems.IsEmpty().Should().BeTrue();

    [Fact]
    public void IsEmpty_ReturnsFalseForNonEmptyCollection() => this.Context.Items.IsEmpty().Should().BeFalse();

    [Theory]
    [InlineData(2, 4)]
    [InlineData(20, 4)]
    [InlineData(40, 4)]
    public void TakeRange_Returns4Items(int startIndex, int count) => this.Context.Items.TakeRange(startIndex, count).Should().HaveCount(4);

    [Theory]
    [InlineData(2, 2)]
    [InlineData(998, 4)]
    public void TakeRange_Returns2Items(int startIndex, int count) => this.Context.Items.TakeRange(startIndex, count).Should().HaveCount(2);

    [Fact]
    public void TakeRange_ReturnsItems_2_3_4_5()
    {
      int startIndex = 2;
      int count = 4;
      _ = this.Context.Items.TakeRange(startIndex, count).Should().Contain(new[] { 2, 3, 4, 5 }, $"StartIndex: {startIndex}; Count: {count}");
    }

    [Fact]
    public void LastOrDefaultInSorted_ReturnsDefaultValueOnFail() => this.Context.Items.LastOrDefaultInSorted(this.Context.FailingContainsPredicate).Should().Be(default, "the predicate has failed to produce a result.");

    [Fact]
    public void LastOrDefaultInSorted_NotThrowExceptionOnFail() => this.Context.Items.Invoking(items => items.LastOrDefaultInSorted(this.Context.FailingContainsPredicate)).Should().NotThrow("the predicate has failed to produce a result and returns a default instead of throwing.");

    [Fact]
    public void LastOrDefaultInSorted_Returns_4_OnSuccess() => this.Context.Items.LastOrDefaultInSorted(this.Context.SuceedingContainsPredicate).Should().Be(4, "the predicate has produced a result.");

    //[Fact]
    //public void LastOrDefaultInSorted_ReturnsFasterThanLastOrDefault_OnSuccess()
    //{
    //  TimeSpan executionTimeLastOrDefaultInSorted = Profiler.LogAverageTime(() => this.Context.Items.LastOrDefaultInSorted(this.Context.SuceedingContainsPredicate), 10000);
    //  TimeSpan executionTimeLastOrDefault = Profiler.LogAverageTime(() => this.Context.Items.LastOrDefault(this.Context.SuceedingContainsPredicate), 10000);
    //  executionTimeLastOrDefaultInSorted.Should().BeLessThanOrEqualTo(executionTimeLastOrDefault);
    //}

    [Fact]
    public void LastInSorted_ThrowExceptionOnFail() => this.Context.Items.Invoking(items => items.LastInSorted(this.Context.FailingContainsPredicate)).Should().ThrowExactly<InvalidOperationException>("the predicate has failed to produce a result and in this case must throw.");

    [Fact]
    public void LastInSorted_Returns_4_OnSuccess() => this.Context.Items.LastInSorted(this.Context.SuceedingContainsPredicate).Should().Be(4, "the predicate has produced a result.");

    //[Fact]
    //public void AddRange__ToCollection_ReturnsOriginalSource()
    //{
    //  this.Context.Items.AddRange(this.Context.NewItems).Should().BeSameAs(this.Context.Items);
    //  this.Context.Reset();
    //}

    //[Fact]
    //public void AddRange_ToCollection_ThrowExceptionOnRangeNull()
    //{
    //  ICollection<int> nullRange = null;
    //  _ = this.Context.Items
    //    .Invoking(source => source.AddRange(nullRange))
    //    .Should().ThrowExactly<ArgumentNullException>("range is NULL.");
    //}

    //[Fact]
    //public void AddRange_Dictionary_ToDictionary_ReturnsOriginalSourceWithCountOfSum_ItemsCount_NewItemsCount()
    //{
    //  this.Context.ItemTable.AddRange(this.Context.NewTableItemsFromDictionary).Should().HaveCount(this.Context.ItemsCount + this.Context.NewItemsCount);
    //  this.Context.Reset();
    //}

    //[Fact]
    //public void AddRange_Dictionary_ToDictionary_ThrowExceptionOnRangeNull()
    //{
    //  IDictionary<int, int> nullRange = null;
    //  _ = this.Context.ItemTable
    //    .Invoking(source => source.AddRange(nullRange))
    //    .Should().ThrowExactly<ArgumentNullException>("range is NULL.");
    //}

    //[Fact]
    //public void AddRange_TupleCollection_ToDictionary_ReturnsOriginalSourceWithCountOfSum_ItemsCount_NewItemsCount()
    //{
    //  this.Context.ItemTable
    //    .AddRange(this.Context.NewTableItemsFromTupleCollection);

    //    .Should().HaveCount(this.Context.ItemsCount + this.Context.NewItemsCount);
    //  this.Context.Reset();
    //}

    //[Fact]
    //public void AddRange_TupleCollection_ToDictionary_ThrowExceptionOnRangeNull()
    //{
    //  IEnumerable<(int, int)> nullRange = null;
    //  this.Context.ItemTable
    //    .Invoking(source => source.AddRange(nullRange))
    //    .Should().ThrowExactly<ArgumentNullException>("range is NULL.");
    //}

    //[Fact]
    //public void AddRange_KeyValuePairCollection_ToDictionary_ReturnsOriginalSourceWithCountOfSum_ItemsCount_NewItemsCount()
    //{
    //  this.Context.ItemTable
    //    .AddRange(this.Context.NewTableItemsFromKeyValuePairCollection)
    //    .Should().HaveCount(this.Context.ItemsCount + this.Context.NewItemsCount);
    //  this.Context.Reset();
    //}

    //[Fact]
    //public void AddRange_KeyValuePairCollection_ToDictionary_ThrowExceptionOnRangeNull()
    //{
    //  IEnumerable<KeyValuePair<int, int>> nullRange = null;
    //  this.Context.ItemTable
    //    .Invoking(source => source.AddRange(nullRange))
    //    .Should().ThrowExactly<ArgumentNullException>("range is NULL.");
    //}

    public void Dispose()
    {
    }
  }
}
