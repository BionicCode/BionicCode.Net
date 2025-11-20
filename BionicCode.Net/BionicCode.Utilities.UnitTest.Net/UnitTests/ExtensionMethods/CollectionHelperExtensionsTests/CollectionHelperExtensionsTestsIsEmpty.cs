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
            this.EmptyIEnumerableT = Enumerable.Empty<int>();
            this.IEnumerableTWith5Items = Enumerable.Range(0, 5);
            this.EmptyIEnumerable = this.EmptyIEnumerableT;
            this.IEnumerableWith5Items = this.IEnumerableTWith5Items;
            this.EmptyList = new List<int>();
            this.ListWith5Items = this.IEnumerableTWith5Items.ToList();
        }

        [Fact]
        public void EmptyIEnumerable_IsEmpty_MustReturnTrue()
        {
            bool isEmpty = this.EmptyIEnumerable.IsEmpty();

            _ = isEmpty.Should().BeTrue();
        }

        [Fact]
        public void NotEmptyIEnumerable_IsEmpty_MustReturnFalse()
        {
            bool isEmpty = this.IEnumerableWith5Items.IsEmpty();

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
            bool isEmpty = this.EmptyIEnumerableT.IsEmpty();

            _ = isEmpty.Should().BeTrue();
        }

        [Fact]
        public void NotEmptyIEnumerableT_IsEmpty_MustReturnFalse()
        {
            bool isEmpty = this.IEnumerableTWith5Items.IsEmpty();

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
            bool isEmpty = this.EmptyList.IsEmpty();

            _ = isEmpty.Should().BeTrue();
        }

        [Fact]
        public void NotEmptyICollectionT_IsEmpty_MustReturnFalse()
        {
            bool isEmpty = this.ListWith5Items.IsEmpty();

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
            bool isEmpty = ((ICollection)this.EmptyList).IsEmpty();

            _ = isEmpty.Should().BeTrue();
        }

        [Fact]
        public void NotEmptyICollection_IsEmpty_MustReturnFalse()
        {
            bool isEmpty = ((ICollection)this.ListWith5Items).IsEmpty();

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
