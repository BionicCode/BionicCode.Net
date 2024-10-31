namespace BionicCode.Utilities.Net.UnitTest.ExtensionMethodsTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using FluentAssertions;
  using Xunit;
  using BionicCode.Utilities.Net;

  public class CollectionHelperExtensionsTestsTakeRange : IClassFixture<TestContext>
  {
    private TestContext Context { get; }

    public CollectionHelperExtensionsTestsTakeRange(TestContext context) 
    {
      this.Context = context;
    }

    [Theory]
    [InlineData(-2, 4)]
    [InlineData(-1, -1)]
    [InlineData(40, -4)]
    [InlineData(TestContext.ItemsCapacity, 1)]
    [InlineData(0, TestContext.ItemsCapacity + 1)]
    [InlineData(1, TestContext.ItemsCapacity)]
    public void TakeRange_MustThrow(int startIndex, int count)
    {
      Action invalidAction = () => this.Context.Items.Take(startIndex, count).Should().HaveCount(count);
      _ = invalidAction.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(2, 0)]
    [InlineData(45, 4)]
    [InlineData(0, TestContext.ItemsCapacity)]
    public void TakeRange_ReturnsNItems(int startIndex, int count) => this.Context.Items.Take(startIndex, count).Should().HaveCount(count);

    [Fact]
    public void TakeRange_ReturnsItemRange_2To5()
    {
      int startIndex = 2;
      int count = 4;
      IEnumerable<int> reference = this.Context.Items.GetRange(startIndex, count);

      IEnumerable<int> result = this.Context.Items.Take(startIndex, count);

      _ = result.Should().BeEquivalentTo(reference, $"StartIndex: {startIndex}; Count: {count}");
    }
  }
}
