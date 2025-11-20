namespace BionicCode.Utilities.Net.UnitTest.ExtensionMethodsTests
{
    using System;
    using BionicCode.Utilities.Net;
    using FluentAssertions;
    using Xunit;

    public class CollectionExtensionMethodTest : IClassFixture<TestContext>, IDisposable
    {
        private TestContext Context { get; }
        public CollectionExtensionMethodTest(TestContext context) => this.Context = context;

        [Fact]
        public void IsEmpty_ReturnsTrueForEmptyCollection() => this.Context.EmptyItems.IsEmpty().Should().BeTrue();

        [Fact]
        public void IsEmpty_ReturnsFalseForNonEmptyCollection() => this.Context.Items.IsEmpty().Should().BeFalse();

        [Fact]
        public void LastOrDefaultInSorted_ReturnsDefaultValueOnFail() => this.Context.Items.LastOrDefaultInSorted(this.Context.FailContainsPredicate).Should().Be(default, "the predicate has failed to produce a result.");

        [Fact]
        public void LastOrDefaultInSorted_NotThrowExceptionOnFail() => this.Context.Items.Invoking(items => items.LastOrDefaultInSorted(this.Context.FailContainsPredicate)).Should().NotThrow("the predicate has failed to produce a result and returns a default instead of throwing.");

        [Fact]
        public void LastOrDefaultInSorted_Returns_4_OnSuccess() => this.Context.Items.LastOrDefaultInSorted(this.Context.SuccessContainsPredicate).Should().Be(4, "the predicate has produced a result.");

        //[Fact]
        //public void LastOrDefaultInSorted_ReturnsFasterThanLastOrDefault_OnSuccess()
        //{
        //  TimeSpan executionTimeLastOrDefaultInSorted = Profiler.LogAverageTime(() => this.Context.Items.LastOrDefaultInSorted(this.Context.SucceedingContainsPredicate), 10000);
        //  TimeSpan executionTimeLastOrDefault = Profiler.LogAverageTime(() => this.Context.Items.LastOrDefault(this.Context.SucceedingContainsPredicate), 10000);
        //  executionTimeLastOrDefaultInSorted.Should().BeLessThanOrEqualTo(executionTimeLastOrDefault);
        //}

        [Fact]
        public void LastInSorted_ThrowExceptionOnFail() => this.Context.Items.Invoking(items => items.LastInSorted(this.Context.FailContainsPredicate)).Should().ThrowExactly<InvalidOperationException>("the predicate has failed to produce a result and in this case must throw.");

        [Fact]
        public void LastInSorted_Returns_4_OnSuccess() => this.Context.Items.LastInSorted(this.Context.SuccessContainsPredicate).Should().Be(4, "the predicate has produced a result.");

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
