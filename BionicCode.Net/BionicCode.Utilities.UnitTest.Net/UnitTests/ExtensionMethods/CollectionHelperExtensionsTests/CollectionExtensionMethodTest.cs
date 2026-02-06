namespace BionicCode.Utilities.Net.UnitTest.ExtensionMethodsTests;

using System;
using BionicCode.Utilities.Net;
using FluentAssertions;
using Xunit;

public class CollectionExtensionMethodTest : IClassFixture<TestContext>, IDisposable
{
    private TestContext Context { get; }
    public CollectionExtensionMethodTest(TestContext context) => Context = context;

    [Fact]
    public void IsEmpty_ReturnsTrueForEmptyCollection() => Context.EmptyItems.IsEmpty().Should().BeTrue();

    [Fact]
    public void IsEmpty_ReturnsFalseForNonEmptyCollection() => Context.Items.IsEmpty().Should().BeFalse();

    [Fact]
    public void LastOrDefaultInSorted_ReturnsDefaultValueOnFail() => Context.Items.LastOrDefaultInSorted(Context.FailContainsPredicate).Should().Be(default, "the predicate has failed to produce a result.");

    [Fact]
    public void LastOrDefaultInSorted_NotThrowExceptionOnFail() => Context.Items.Invoking(items => items.LastOrDefaultInSorted(Context.FailContainsPredicate)).Should().NotThrow("the predicate has failed to produce a result and returns a default instead of throwing.");

    [Fact]
    public void LastOrDefaultInSorted_Returns_4_OnSuccess() => Context.Items.LastOrDefaultInSorted(Context.SuccessContainsPredicate).Should().Be(4, "the predicate has produced a result.");

    //[Fact]
    //public void LastOrDefaultInSorted_ReturnsFasterThanLastOrDefault_OnSuccess()
    //{
    //  TimeSpan executionTimeLastOrDefaultInSorted = Profiler.LogAverageTime(() => Context.Items.LastOrDefaultInSorted(Context.SucceedingContainsPredicate), 10000);
    //  TimeSpan executionTimeLastOrDefault = Profiler.LogAverageTime(() => Context.Items.LastOrDefault(Context.SucceedingContainsPredicate), 10000);
    //  executionTimeLastOrDefaultInSorted.Should().BeLessThanOrEqualTo(executionTimeLastOrDefault);
    //}

    [Fact]
    public void LastInSorted_ThrowExceptionOnFail() => Context.Items.Invoking(items => items.LastInSorted(Context.FailContainsPredicate)).Should().ThrowExactly<InvalidOperationException>("the predicate has failed to produce a result and in this case must throw.");

    [Fact]
    public void LastInSorted_Returns_4_OnSuccess() => Context.Items.LastInSorted(Context.SuccessContainsPredicate).Should().Be(4, "the predicate has produced a result.");

    //[Fact]
    //public void AddRange__ToCollection_ReturnsOriginalSource()
    //{
    //  Context.Items.AddRange(Context.NewItems).Should().BeSameAs(Context.Items);
    //  Context.Reset();
    //}

    //[Fact]
    //public void AddRange_ToCollection_ThrowExceptionOnRangeNull()
    //{
    //  ICollection<int> nullRange = null;
    //  _ = Context.Items
    //    .Invoking(source => source.AddRange(nullRange))
    //    .Should().ThrowExactly<ArgumentNullException>("range is NULL.");
    //}

    //[Fact]
    //public void AddRange_Dictionary_ToDictionary_ReturnsOriginalSourceWithCountOfSum_ItemsCount_NewItemsCount()
    //{
    //  Context.ItemTable.AddRange(Context.NewTableItemsFromDictionary).Should().HaveCount(Context.ItemsCount + Context.NewItemsCount);
    //  Context.Reset();
    //}

    //[Fact]
    //public void AddRange_Dictionary_ToDictionary_ThrowExceptionOnRangeNull()
    //{
    //  IDictionary<int, int> nullRange = null;
    //  _ = Context.ItemTable
    //    .Invoking(source => source.AddRange(nullRange))
    //    .Should().ThrowExactly<ArgumentNullException>("range is NULL.");
    //}

    //[Fact]
    //public void AddRange_TupleCollection_ToDictionary_ReturnsOriginalSourceWithCountOfSum_ItemsCount_NewItemsCount()
    //{
    //  Context.ItemTable
    //    .AddRange(Context.NewTableItemsFromTupleCollection);

    //    .Should().HaveCount(Context.ItemsCount + Context.NewItemsCount);
    //  Context.Reset();
    //}

    //[Fact]
    //public void AddRange_TupleCollection_ToDictionary_ThrowExceptionOnRangeNull()
    //{
    //  IEnumerable<(int, int)> nullRange = null;
    //  Context.ItemTable
    //    .Invoking(source => source.AddRange(nullRange))
    //    .Should().ThrowExactly<ArgumentNullException>("range is NULL.");
    //}

    //[Fact]
    //public void AddRange_KeyValuePairCollection_ToDictionary_ReturnsOriginalSourceWithCountOfSum_ItemsCount_NewItemsCount()
    //{
    //  Context.ItemTable
    //    .AddRange(Context.NewTableItemsFromKeyValuePairCollection)
    //    .Should().HaveCount(Context.ItemsCount + Context.NewItemsCount);
    //  Context.Reset();
    //}

    //[Fact]
    //public void AddRange_KeyValuePairCollection_ToDictionary_ThrowExceptionOnRangeNull()
    //{
    //  IEnumerable<KeyValuePair<int, int>> nullRange = null;
    //  Context.ItemTable
    //    .Invoking(source => source.AddRange(nullRange))
    //    .Should().ThrowExactly<ArgumentNullException>("range is NULL.");
    //}

    public void Dispose()
    {
    }
}
