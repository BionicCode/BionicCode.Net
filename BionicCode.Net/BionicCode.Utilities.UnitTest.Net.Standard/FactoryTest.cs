namespace BionicCode.Utilities.Net.UnitTest
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using Xunit;
  using FluentAssertions;
  using BionicCode.Utilities.Net;
  using BionicCode.Utilities.Net.UnitTest.Resources;
  using FluentAssertions.Equivalency;

  public class FactoryTest : IDisposable
{
  PersonFactory PersonFactory { get; }

  public string PersonFirstName => "FirstName";
  public string PersonLastName => "LastName";
  public int PersonId => 1;
  public FactoryTest()
  {
    this.PersonFactory = new PersonFactory();
  }

  [Fact]
  public void DefaultFactoryModeIsSingleton()
  {
    this.PersonFactory.FactoryMode.Should().Be(FactoryMode.Singleton);
  }

  [Fact]
  public void EnteringFactoryScopeMustSetFactoryModeToScoped()
  {
    this.PersonFactory.FactoryMode = FactoryMode.Transient;
    using (IDisposable scope = this.PersonFactory.CreateScope())
    {
      this.PersonFactory.FactoryMode.Should().Be(FactoryMode.Scoped);
    }
  }

    [Fact]
    public void LeavingFactoryScopeMustRevertFactoryModeFromScopedToPrevious()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Transient;
      using (IDisposable scope = this.PersonFactory.CreateScope())
      {
      }

      this.PersonFactory.FactoryMode.Should().Be(FactoryMode.Transient);
    }

    [Fact]
    public void CallToParameterlessCreateProducesDefaultPerson()
    {
      Person referencePerson = CreateReferenceDefaultPerson();
      this.PersonFactory.Create().Should().BeEquivalentTo(referencePerson);
    }

    [Fact]
    public void CallToParameterizedCreateNotProducesDefaultPerson()
    {
      Person referencePerson = CreateReferenceDefaultPerson();
      this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId).Should().NotBeEquivalentTo(referencePerson);
    }

    [Fact]
    public void CallToParameterizedCreateProducesNonDefaultPerson()
    {
      Person referencePerson = CreateReferencePerson();
      this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId).Should().BeEquivalentTo(referencePerson);
    }

    [Fact]
    public void CallingParameterlessCreateMultipleTimesProducesSameInstanceInSingletonMode()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Singleton;
      Person referencePerson = CreateSingletonReferenceDefaultPersonFromFactory();
      var instances = new List<Person>();
      for (int count = 0; count < 10; count++)
      {
        instances.Add(this.PersonFactory.Create());
      }

      instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePerson).Should().BeTrue());
    }

    [Fact]
    public void CallingParameterizedCreateMultipleTimesProducesSameInstanceInSingletonMode()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Singleton;
      Person referencePerson = CreateSingletonReferencePersonFromFactory();
      var instances = new List<Person>();
      for (int count = 0; count < 10; count++)
      {
        instances.Add(this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId));
      }

      instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePerson).Should().BeTrue());
    }

    [Fact]
    public void CallingParameterlessCreateMultipleTimesProducesNewInstanceInTransientMode()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Transient;
      Person referencePerson = CreateSingletonReferenceDefaultPersonFromFactory();
      var instances = new List<Person>();
      for (int count = 0; count < 10; count++)
      {
        instances.Add(this.PersonFactory.Create());
      }

      instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePerson).Should().BeFalse());
    }

    [Fact]
    public void CallingParameterizedCreateWithMultipleTimesProducesNewInstanceInTransientMode()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Transient;
      Person referencePerson = CreateSingletonReferencePersonFromFactory();
      var instances = new List<Person>();
      for (int count = 0; count < 10; count++)
      {
        instances.Add(this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId));
      }

      instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePerson).Should().BeFalse());
    }

    [Fact]
    public void CallingParameterlessCreateMultipleTimesProducesSameInstanceInScope()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Singleton;
      var instances = new List<Person>();
      using (IDisposable scope = this.PersonFactory.CreateScope())
      {
        Person referencePerson = CreateSingletonReferenceDefaultPersonFromFactory();
        for (int count = 0; count < 10; count++)
        {
          instances.Add(this.PersonFactory.Create());
        }
        instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePerson).Should().BeTrue());
      }
    }

    [Fact]
    public void CallingParameterizedCreateMultipleTimesProducesSameInstanceInScope()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Singleton;
      var instances = new List<Person>();
      using (IDisposable scope = this.PersonFactory.CreateScope())
      {
        Person referencePerson = CreateSingletonReferencePersonFromFactory();
        for (int count = 0; count < 10; count++)
        {
          instances.Add(this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId));
        }

        instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePerson).Should().BeTrue());
      }
    }

    [Fact]
    public void CallingParameterlessCreateProducedInstancesInScopeThatDifferFromSharedInstancesProducedOutsideOfScope()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Singleton;
      Person referencePersonBeforeScope = this.PersonFactory.Create();
      var instances = new List<Person>();
      using (IDisposable scope = this.PersonFactory.CreateScope())
      {
        for (int count = 0; count < 10; count++)
        {
          instances.Add(this.PersonFactory.Create());
        }
      }

      Person referencePersonAfterScope = this.PersonFactory.Create();

      instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePersonBeforeScope).Should().BeFalse());
      instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePersonAfterScope).Should().BeFalse());
    }

    [Fact]
    public void CallingParameterlessCreateProducesSameInstanceBeforeAndAfterScopeInSingletonMode()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Singleton;
      Person referencePersonBeforeScope = this.PersonFactory.Create();
      var instances = new List<Person>();
      using (IDisposable scope = this.PersonFactory.CreateScope())
      {
        for (int count = 0; count < 10; count++)
        {
          instances.Add(this.PersonFactory.Create());
        }
      }

      Person referencePersonAfterScope = this.PersonFactory.Create();
      ReferenceEquals(referencePersonBeforeScope, referencePersonAfterScope).Should().BeTrue();
    }

    [Fact]
    public void CallingParameterizedCreateProducedInstancesInScopeThatDifferFromSharedInstancesProducedOutsideOfScope()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Singleton;
      Person referencePersonBeforeScope = this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId);
      var instances = new List<Person>();
      using (IDisposable scope = this.PersonFactory.CreateScope())
      {
        for (int count = 0; count < 10; count++)
        {
          instances.Add(this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId));
        }
      }

      Person referencePersonAfterScope = this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId);

      instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePersonBeforeScope).Should().BeFalse());
      instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePersonAfterScope).Should().BeFalse());
    }

    [Fact]
    public void CallingParameterizedCreateProducesSameInstanceBeforeAndAfterScopInSingletonMode()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Singleton;
      Person referencePersonBeforeScope = this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId);
      var instances = new List<Person>();
      using (IDisposable scope = this.PersonFactory.CreateScope())
      {
        for (int count = 0; count < 10; count++)
        {
          instances.Add(this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId));
        }
      }

      Person referencePersonAfterScope = this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId);
      ReferenceEquals(referencePersonBeforeScope, referencePersonAfterScope).Should().BeTrue();
    }

    [Fact]
    public void CallingParameterlessCreateProducesDifferentInstanceBeforeAndAfterScopInTransientMode()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Transient;
      Person referencePersonBeforeScope = this.PersonFactory.Create();
      var instances = new List<Person>();
      using (IDisposable scope = this.PersonFactory.CreateScope())
      {
        for (int count = 0; count < 10; count++)
        {
          instances.Add(this.PersonFactory.Create());
        }
      }

      Person referencePersonAfterScope = this.PersonFactory.Create();
      ReferenceEquals(referencePersonBeforeScope, referencePersonAfterScope).Should().BeFalse();
    }

    [Fact]
    public void CallingParameterizedCreateProducesDifferentInstanceBeforeAndAfterScopInTransientMode()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Transient;
      Person referencePersonBeforeScope = this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId);
      var instances = new List<Person>();
      using (IDisposable scope = this.PersonFactory.CreateScope())
      {
        for (int count = 0; count < 10; count++)
        {
          instances.Add(this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId));
        }
      }

      Person referencePersonAfterScope = this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId);
      ReferenceEquals(referencePersonBeforeScope, referencePersonAfterScope).Should().BeFalse();
    }

    [Fact]
    public void ChangingFactoryModeInScopeWillThrowException()
    {
      this.PersonFactory.FactoryMode = FactoryMode.Singleton;
      this.Invoking(_this =>
      {
        var instances = new List<Person>();
        using (IDisposable scope = _this.PersonFactory.CreateScope())
        {
          this.PersonFactory.FactoryMode = FactoryMode.Transient;
          Person referencePerson = CreateSingletonReferencePersonFromFactory();
          for (int count = 0; count < 10; count++)
          {
            instances.Add(_this.PersonFactory.Create(_this.PersonFirstName, _this.PersonLastName, _this.PersonId));
          }
        }
      }).Should().ThrowExactly<InvalidOperationException>("changing the IFactory.FactoryMode property inside a scope is not allowed.");
    }

    private Person CreateReferencePerson() => new Person(this.PersonFirstName, this.PersonLastName, this.PersonId);

    private Person CreateSingletonReferencePersonFromFactory() => this.PersonFactory.Create(this.PersonFirstName, this.PersonLastName, this.PersonId);

    private Person CreateReferenceDefaultPerson() => new Person(this.PersonFactory.DefaultPersonFirstName, this.PersonFactory.DefaultPersonLastName, this.PersonFactory.DefaultPersonId);

    private Person CreateSingletonReferenceDefaultPersonFromFactory() => this.PersonFactory.Create(this.PersonFactory.DefaultPersonFirstName, this.PersonFactory.DefaultPersonLastName, this.PersonFactory.DefaultPersonId);

    public void Dispose()
    { }
  }
}
