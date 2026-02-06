namespace BionicCode.Utilities.Net.UnitTest.ExtensionMethodsTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class CollectionHelperExtensionsTestsIsEmpty
    {
        public List<int> EmptyList { get; set; }
        public List<int> ListWith5Items { get; set; }
        public IEnumerable<int> EmptyIEnumerableT { get; set; }
        public IEnumerable<int> IEnumerableTWith5Items { get; set; }
        public IEnumerable EmptyIEnumerable { get; set; }
        public IEnumerable IEnumerableWith5Items { get; set; }

        public CollectionHelperExtensionsTestsIsEmpty()
        {
            EmptyIEnumerableT = Enumerable.Empty<int>();
            IEnumerableTWith5Items = Enumerable.Range(0, 5);
            EmptyIEnumerable = EmptyIEnumerableT;
            IEnumerableWith5Items = IEnumerableTWith5Items;
            EmptyList = new List<int>();
            ListWith5Items = IEnumerableTWith5Items.ToList();
        }

        [Fact]
        public void EmptyIEnumerable_IsEmpty_MustReturnTrue()
        {
            bool isEmpty = EmptyIEnumerable.IsEmpty();

            _ = isEmpty.Should().BeTrue();
        }

        [Fact]
        public void NotEmptyIEnumerable_IsEmpty_MustReturnFalse()
        {
            bool isEmpty = IEnumerableWith5Items.IsEmpty();

            _ = isEmpty.Should().BeFalse();
        }

        [Fact]
        public void IEnumerableCallExtensionMethodDirectly_PassingNull_MustThrow()
        {
            Action action = () => HelperExtensionsCommon.IsEmpty<int>(null);

            _ = action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void EmptyIEnumerableT_IsEmpty_MustReturnTrue()
        {
            bool isEmpty = EmptyIEnumerableT.IsEmpty();

            _ = isEmpty.Should().BeTrue();
        }

        [Fact]
        public void NotEmptyIEnumerableT_IsEmpty_MustReturnFalse()
        {
            bool isEmpty = IEnumerableTWith5Items.IsEmpty();

            _ = isEmpty.Should().BeFalse();
        }

        [Fact]
        public void IEnumerableTCallExtensionMethodDirectly_PassingNull_MustThrow()
        {
            Action action = () => HelperExtensionsCommon.IsEmpty<int>(null);

            _ = action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void EmptyICollectionT_IsEmpty_MustReturnTrue()
        {
            bool isEmpty = EmptyList.IsEmpty();

            _ = isEmpty.Should().BeTrue();
        }

        [Fact]
        public void NotEmptyICollectionT_IsEmpty_MustReturnFalse()
        {
            bool isEmpty = ListWith5Items.IsEmpty();

            _ = isEmpty.Should().BeFalse();
        }

        [Fact]
        public void ICollectionTCallExtensionMethodDirectly_PassingNull_MustThrow()
        {
            Action action = () => HelperExtensionsCommon.IsEmpty<int>(null);

            _ = action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void EmptyICollection_IsEmpty_MustReturnTrue()
        {
            bool isEmpty = ((ICollection)EmptyList).IsEmpty();

            _ = isEmpty.Should().BeTrue();
        }

        [Fact]
        public void NotEmptyICollection_IsEmpty_MustReturnFalse()
        {
            bool isEmpty = ((ICollection)ListWith5Items).IsEmpty();

            _ = isEmpty.Should().BeFalse();
        }

        [Fact]
        public void ICollectionCallExtensionMethodDirectly_PassingNull_MustThrow()
        {
            Action action = () => HelperExtensionsCommon.IsEmpty((ICollection)null);

            _ = action.Should().ThrowExactly<ArgumentNullException>();
        }
    }
}
