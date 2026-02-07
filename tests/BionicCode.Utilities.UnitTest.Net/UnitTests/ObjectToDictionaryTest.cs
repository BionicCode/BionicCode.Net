namespace BionicCode.Utilities.Net.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BionicCode.Utilities.Net;
    using BionicCode.Utilities.Net.UnitTest.Resources;
    using FluentAssertions;
    using Xunit;

    public class TestObject
    {
        public int ItemsCount { get; }
        public ICollection<int> Items { get; private set; }
        public IList<Person> Persons { get; private set; }
        public Dictionary<int, int> ItemTable { get; private set; }
        public ICollection<int> EmptyItems { get; }
        public Func<int, bool> SuceedingContainsPredicate => item => item < 5;

        [IgnoreInObjectGraph]
        public Dictionary<string, object> DeepObjectGraph { get; private set; }

        [IgnoreInObjectGraph]
        public Dictionary<string, object> FlatObjectGraph { get; private set; }

        [IgnoreInObjectGraph]
        public bool IgnoredProperty { get; set; }

        public static bool StaticProperty { get; set; }

        private bool PrivateProperty { get; set; }
        protected bool ProtectedProperty { get; set; }
        public bool PublicProperty { get; set; }
        internal bool InternalProperty { get; set; }

        public TestObject()
        {
            ItemsCount = 1000;

            Items = new List<int>();
            EmptyItems = new List<int>();
            Persons = new List<Person>();

            ItemTable = new Dictionary<int, int>();

            for (int count = 0; count < ItemsCount; count++)
            {
                int key = count;
                int value = count * 10;
                ItemTable.Add(key, value);
                Items.Add(count);
                Persons.Add(new Person(((char)key).ToString(), ((char)value).ToString(), key));
            }

            Initialize();
        }

        public void Initialize()
        {
            DeepObjectGraph = ToDictionary(includeNonPublicMembers: true);
            FlatObjectGraph = ToFlatDictionary(includeNonPublicMembers: true);
        }

        public int Return12() => 12;
    }

    public class ObjectToDictionaryTest : IClassFixture<TestObject>, IDisposable
    {
        private TestObject Object { get; }
        public ObjectToDictionaryTest(TestObject context) => Object = context;

        [Fact]
        public void ReturnsDictionaryFromObject() => _ = Object.ToDictionary(includeNonPublicMembers: true)
            .Should().BeOfType(typeof(Dictionary<string, object>));

        [Fact]
        public void ReturnsDictionaryOfDictionariesFromObjectIgnoringDecoratedProperties() => _ = Object.DeepObjectGraph
            .Should().NotContainKey(nameof(Object.IgnoredProperty));

        [Fact]
        public void ReturnsDictionaryOfDictionariesFromObjectIgnoringPrivateProperties() => _ = Object.DeepObjectGraph
            .Should().NotContainKey("PrivateProperty");

        [Fact]
        public void ReturnsDictionaryOfDictionariesFromObjectIgnoringProtectedProperties() => _ = Object.DeepObjectGraph
            .Should().NotContainKey("ProtectedProperty");

        [Fact]
        public void ReturnsDictionaryOfDictionariesFromObjectIgnoringInternalProperties() => _ = Object.DeepObjectGraph
            .Should().NotContainKey(nameof(Object.InternalProperty));

        [Fact]
        public void ReturnsDictionaryOfDictionariesFromObjectIncludingStaticProperties() => _ = Object.DeepObjectGraph
            .Should().ContainKey(nameof(TestObject.StaticProperty));

        [Fact]
        public void ReturnsDictionaryOfDictionariesFromObjectIncludingPublicProperties() => _ = Object.DeepObjectGraph
            .Should().ContainKey(nameof(Object.PublicProperty));

        [Fact]
        public void ReturnsDictionaryOfDictionariesFromObject() => _ = Object.DeepObjectGraph[nameof(Object.Items)]
            .Should().BeOfType(typeof(Dictionary<string, object>));

        [Fact]
        public void ReturnsDictionaryFromObjectAndInvokeDelegate()
        {
            _ = Object.DeepObjectGraph[nameof(Object.SuceedingContainsPredicate)]
              .As<Func<int, bool>>()
              .Invoke(4)
              .Should().BeTrue();
            _ = Object.DeepObjectGraph[nameof(Object.SuceedingContainsPredicate)]
              .As<Func<int, bool>>()
              .Invoke(100)
              .Should().BeFalse();
        }

        [Fact]
        public void ReturnsDictionaryFromObjectAndCollectionsMustHaveOriginalCount() => _ = Object.DeepObjectGraph[nameof(Object.Items)]
            .As<IDictionary<string, object>>()
            .Should().HaveCount(Object.ItemsCount);

        [Fact]
        public void ReturnsDictionaryFromObjectAndCollectionsMustContain_4_AtIndex_5() => _ = Object.DeepObjectGraph[nameof(Object.Items)]
            .As<IDictionary<string, object>>()["5"]
            .Should().BeEquivalentTo(Object.Items.ElementAt(5));

        [Fact]
        public void ReturnsDictionaryFromObjectAndCollectionsMustContain_DictionariesOfPerson() => _ = Object.DeepObjectGraph[nameof(Object.Persons)]
            .As<IDictionary<string, object>>()["5"]
            .As<IDictionary<string, object>>()["Id"]
            .Should().BeEquivalentTo(Object.Persons[5].Id);

        [Fact]
        public void ReturnsFlattenedDictionaryFromObjectAndCollectionsMustContain_4_AtIndex_5() => _ = Object.FlatObjectGraph[nameof(Object.Items)]
            .As<IList<int>>()[5]
            .Should().Be(Object.Items.ElementAt(5));

        [Fact]
        public void ReturnsFlattenedDictionaryOfOriginalTypedValuesFromObject()
        {
            _ = Object.FlatObjectGraph[nameof(Object.Items)]
              .Should().BeOfType(Object.Items.GetType());
            _ = Object.FlatObjectGraph[nameof(Object.Items)]
              .Should().NotBeOfType(typeof(IDictionary<string, object>));
        }

        [Fact]
        public void ReturnsFlattenedDictionaryFromObjectAndCollectionsMustContain_Persons() => _ = Object.FlatObjectGraph[nameof(Object.Persons)]
            .As<IList<Person>>()[5].Id
            .Should().Be(Object.Persons[5].Id);

        [Fact]
        public void ReturnsFlattenedDictionaryFromObjectAndInvokeDelegate()
        {
            _ = Object.FlatObjectGraph[nameof(Object.SuceedingContainsPredicate)]
              .As<Func<int, bool>>()
              .Invoke(4)
              .Should().BeTrue();
            _ = Object.FlatObjectGraph[nameof(Object.SuceedingContainsPredicate)]
              .As<Func<int, bool>>()
              .Invoke(100)
              .Should().BeFalse();
        }

        public void Dispose()
        {
        }
    }
}
