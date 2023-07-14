namespace BionicCode.Utilities.UnitTest.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Xunit;
  using BionicCode.Utilities.Net;
  using FluentAssertions;
  using BionicCode.Utilities.UnitTest.Net.Resources;

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
      this.ItemsCount = 1000;

      this.Items = new List<int>();
      this.EmptyItems = new List<int>();
      this.Persons = new List<Person>();

      this.ItemTable = new Dictionary<int, int>();

      for (int count = 0; count < this.ItemsCount; count++)
      {
        int key = count;
        int value = count * 10;
        this.ItemTable.Add(key, value);
        this.Items.Add(count);
        this.Persons.Add(new Person(((char)key).ToString(), ((char)value).ToString(), key));
      }

      Initialize();
    }

    public void Initialize()
    {
      this.DeepObjectGraph = this.ToDictionary();
      this.FlatObjectGraph = this.ToFlatDictionary();
    }

    public int Return12() => 12;
  }

  public class ObjectToDictionaryTest : IClassFixture<TestObject>, IDisposable
  {
    private TestObject Object { get; }
    public ObjectToDictionaryTest(TestObject context)
    {
      this.Object = context;
    }

    [Fact]
    public void ReturnsDictionaryFromObject()
    {
      this.Object.ToDictionary()
        .Should().BeOfType(typeof(Dictionary<string, object>));
    }

    [Fact]
    public void ReturnsDictionaryOfDictionariesFromObjectIgnoringDecoratedProperties()
    {
      this.Object.DeepObjectGraph
        .Should().NotContainKey(nameof(this.Object.IgnoredProperty));
    }

    [Fact]
    public void ReturnsDictionaryOfDictionariesFromObjectIgnoringPrivateProperties()
    {
      this.Object.DeepObjectGraph
        .Should().NotContainKey("PrivateProperty");
    }

    [Fact]
    public void ReturnsDictionaryOfDictionariesFromObjectIgnoringProtectedProperties()
    {
      this.Object.DeepObjectGraph
        .Should().NotContainKey("ProtectedProperty");
    }

    [Fact]
    public void ReturnsDictionaryOfDictionariesFromObjectIgnoringInternalProperties()
    {
      this.Object.DeepObjectGraph
        .Should().NotContainKey(nameof(this.Object.InternalProperty));
    }

    [Fact]
    public void ReturnsDictionaryOfDictionariesFromObjectIncludingStaticProperties()
    {
      this.Object.DeepObjectGraph
        .Should().ContainKey(nameof(TestObject.StaticProperty));
    }

    [Fact]
    public void ReturnsDictionaryOfDictionariesFromObjectIncludingPublicProperties()
    {
      this.Object.DeepObjectGraph
        .Should().ContainKey(nameof(this.Object.PublicProperty));
    }

    [Fact]
    public void ReturnsDictionaryOfDictionariesFromObject()
    {
      this.Object.DeepObjectGraph[nameof(this.Object.Items)]
        .Should().BeOfType(typeof(Dictionary<string, object>));
    }

    [Fact]
    public void ReturnsDictionaryFromObjectAndInvokeDelegate()
    {
      this.Object.DeepObjectGraph[nameof(this.Object.SuceedingContainsPredicate)]
        .As<Func<int, bool>>()
        .Invoke(4)
        .Should().BeTrue();
      this.Object.DeepObjectGraph[nameof(this.Object.SuceedingContainsPredicate)]
        .As<Func<int, bool>>()
        .Invoke(100)
        .Should().BeFalse();
    }

    [Fact]
    public void ReturnsDictionaryFromObjectAndCollectionsMustHaveOriginalCount()
    {
      this.Object.DeepObjectGraph[nameof(this.Object.Items)]
        .As<IDictionary<string, object>>()
        .Should().HaveCount(this.Object.ItemsCount);
    }

    [Fact]
    public void ReturnsDictionaryFromObjectAndCollectionsMustContain_4_AtIndex_5()
    {
      this.Object.DeepObjectGraph[nameof(this.Object.Items)]
        .As<IDictionary<string, object>>()["5"]
        .Should().BeEquivalentTo(this.Object.Items.ElementAt(5));
    }

    [Fact]
    public void ReturnsDictionaryFromObjectAndCollectionsMustContain_DictionariesOfPerson()
    {
      this.Object.DeepObjectGraph[nameof(this.Object.Persons)]
        .As<IDictionary<string, object>>()["5"]
        .As<IDictionary<string, object>>()["Id"]
        .Should().BeEquivalentTo(this.Object.Persons[5].Id);
    }

    [Fact]
    public void ReturnsFlattenedDictionaryFromObjectAndCollectionsMustContain_4_AtIndex_5()
    {
      this.Object.FlatObjectGraph[nameof(this.Object.Items)]
        .As<IList<int>>()[5]
        .Should().Be(this.Object.Items.ElementAt(5));
    }

    [Fact]
    public void ReturnsFlattenedDictionaryOfOriginalTypedValuesFromObject()
    {
      this.Object.FlatObjectGraph[nameof(this.Object.Items)]
        .Should().BeOfType(this.Object.Items.GetType());
      this.Object.FlatObjectGraph[nameof(this.Object.Items)]
        .Should().NotBeOfType(typeof(IDictionary<string, object>));
    }

    [Fact]
    public void ReturnsFlattenedDictionaryFromObjectAndCollectionsMustContain_Persons()
    {
      this.Object.FlatObjectGraph[nameof(this.Object.Persons)]
        .As<IList<Person>>()[5].Id
        .Should().Be(this.Object.Persons[5].Id);
    }

    [Fact]
    public void ReturnsFlattenedDictionaryFromObjectAndInvokeDelegate()
    {
      this.Object.FlatObjectGraph[nameof(this.Object.SuceedingContainsPredicate)]
        .As<Func<int, bool>>()
        .Invoke(4)
        .Should().BeTrue();
      this.Object.FlatObjectGraph[nameof(this.Object.SuceedingContainsPredicate)]
        .As<Func<int, bool>>()
        .Invoke(100)
        .Should().BeFalse();
    }

    public void Dispose()
    {
    }
  }
}
