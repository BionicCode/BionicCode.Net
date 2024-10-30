namespace BionicCode.Utilities.Net.UnitTest.UnitTests.ArrayHelper
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using FluentAssertions;
  using Xunit;

  public class ArrayExMoveTests
  {
    private int[] array;
    private readonly int[] referenceArray;
    private const int ArrayLength = 16;
    private const int RangeStart = 0;

    public ArrayExMoveTests()
    {
      this.array = Enumerable.Range(RangeStart, ArrayLength).ToArray();
      this.referenceArray = Enumerable.Range(RangeStart, ArrayLength).ToArray();
    }

    [Fact]
    public void MoveSingleElement_LastToBeginning_MustShiftAllElementsBy1()
    {
      int[] expectedResult = this.referenceArray
        .Select(element => element - 1)
        .ToArray();
      expectedResult[0] = this.referenceArray.Last();

      ArrayEx.Move(ref this.array, ArrayLength - 1, 0);

      _ = this.array.Should().BeEquivalentTo(expectedResult);
    }
  }
}
