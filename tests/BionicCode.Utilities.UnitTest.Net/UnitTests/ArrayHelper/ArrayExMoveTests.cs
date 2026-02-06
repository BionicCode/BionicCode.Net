namespace BionicCode.Utilities.Net.UnitTest.UnitTests.ArrayHelper
{
    using System.Linq;
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
            array = Enumerable.Range(RangeStart, ArrayLength).ToArray();
            referenceArray = Enumerable.Range(RangeStart, ArrayLength).ToArray();
        }

        [Fact]
        public void MoveSingleElement_LastToBeginning_MustShiftAllElementsBy1()
        {
            int[] expectedResult = referenceArray
              .Select(element => element - 1)
              .ToArray();
            expectedResult[0] = referenceArray.Last();

            ArrayEx.Move(ref array, ArrayLength - 1, 0);

            _ = array.Should().BeEquivalentTo(expectedResult);
        }
    }
}
