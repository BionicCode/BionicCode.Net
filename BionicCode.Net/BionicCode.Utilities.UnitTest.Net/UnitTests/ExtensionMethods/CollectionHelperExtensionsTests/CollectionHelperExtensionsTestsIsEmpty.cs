namespace BionicCode.Utilities.Net.UnitTest.ExtensionMethodsTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using FluentAssertions;
  using Xunit;

  public class CollectionHelperExtensionsTestsIsEmpty
  {
    public List<int> EmptyList { get; set; }
    public List<int> ListWith5Items { get; set; }

    public CollectionHelperExtensionsTestsIsEmpty() 
    {
      this.EmptyList = new List<int>();
      this.ListWith5Items = Enumerable.Range(0, 5).ToList();
    }

    [Fact]
    public void EmptyCollection_IsEmpty_MustReturnTrue()
    {
      bool isEmpty = this.EmptyList.IsEmpty();

      _ = isEmpty.Should().BeTrue();
    }

    [Fact]
    public void NotEmptyCollection_IsEmpty_MustReturnFalse()
    {
      bool isEmpty = this.ListWith5Items.IsEmpty();

      _ = isEmpty.Should().BeFalse();
    }

    [Fact]
    public void CallExtensionMethodDirectly_PassingNull_MustThrow()
    {
      Action action = () => HelperExtensionsCommon.IsEmpty<int>(null);

      _ = action.Should().ThrowExactly<ArgumentNullException>();
    }
  }
}
