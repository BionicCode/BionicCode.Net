namespace BionicCode.Utilities.Net.UnitTest
{
    using System;
    using System.Collections.Generic;
    using BionicCode.Utilities.Net;
    using BionicCode.Utilities.Net.UnitTest.Resources;
    using FluentAssertions;
    using Xunit;

    public class FactoryTest : IDisposable
    {
        private PersonFactory PersonFactory { get; }

        public string PersonFirstName => "FirstName";
        public string PersonLastName => "LastName";
        public int PersonId => 1;
        public FactoryTest() => PersonFactory = new PersonFactory();

        [Fact]
        public void DefaultFactoryModeIsSingleton() => PersonFactory.FactoryMode.Should().Be(FactoryMode.Singleton);

        [Fact]
        public void EnteringFactoryScopeMustSetFactoryModeToScoped()
        {
            PersonFactory.FactoryMode = FactoryMode.Transient;
            using IDisposable scope = PersonFactory.CreateScope();
            _ = PersonFactory.FactoryMode.Should().Be(FactoryMode.Scoped);
        }

        [Fact]
        public void LeavingFactoryScopeMustRevertFactoryModeFromScopedToPrevious()
        {
            PersonFactory.FactoryMode = FactoryMode.Transient;
            using (IDisposable scope = PersonFactory.CreateScope())
            {
            }

            _ = PersonFactory.FactoryMode.Should().Be(FactoryMode.Transient);
        }

        [Fact]
        public void CallToParameterlessCreateProducesDefaultPerson()
        {
            Person referencePerson = CreateReferenceDefaultPerson();
            _ = PersonFactory.Create().Should().BeEquivalentTo(referencePerson);
        }

        [Fact]
        public void CallToParameterizedCreateNotProducesDefaultPerson()
        {
            Person referencePerson = CreateReferenceDefaultPerson();
            _ = PersonFactory.Create(PersonFirstName, PersonLastName, PersonId).Should().NotBeEquivalentTo(referencePerson);
        }

        [Fact]
        public void CallToParameterizedCreateProducesNonDefaultPerson()
        {
            Person referencePerson = CreateReferencePerson();
            _ = PersonFactory.Create(PersonFirstName, PersonLastName, PersonId).Should().BeEquivalentTo(referencePerson);
        }

        [Fact]
        public void CallingParameterlessCreateMultipleTimesProducesSameInstanceInSingletonMode()
        {
            PersonFactory.FactoryMode = FactoryMode.Singleton;
            Person referencePerson = CreateSingletonReferenceDefaultPersonFromFactory();
            var instances = new List<Person>();
            for (int count = 0; count < 10; count++)
            {
                instances.Add(PersonFactory.Create());
            }

            _ = instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePerson).Should().BeTrue());
        }

        [Fact]
        public void CallingParameterizedCreateMultipleTimesProducesSameInstanceInSingletonMode()
        {
            PersonFactory.FactoryMode = FactoryMode.Singleton;
            Person referencePerson = CreateSingletonReferencePersonFromFactory();
            var instances = new List<Person>();
            for (int count = 0; count < 10; count++)
            {
                instances.Add(PersonFactory.Create(PersonFirstName, PersonLastName, PersonId));
            }

            _ = instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePerson).Should().BeTrue());
        }

        [Fact]
        public void CallingParameterlessCreateMultipleTimesProducesNewInstanceInTransientMode()
        {
            PersonFactory.FactoryMode = FactoryMode.Transient;
            Person referencePerson = CreateSingletonReferenceDefaultPersonFromFactory();
            var instances = new List<Person>();
            for (int count = 0; count < 10; count++)
            {
                instances.Add(PersonFactory.Create());
            }

            _ = instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePerson).Should().BeFalse());
        }

        [Fact]
        public void CallingParameterizedCreateWithMultipleTimesProducesNewInstanceInTransientMode()
        {
            PersonFactory.FactoryMode = FactoryMode.Transient;
            Person referencePerson = CreateSingletonReferencePersonFromFactory();
            var instances = new List<Person>();
            for (int count = 0; count < 10; count++)
            {
                instances.Add(PersonFactory.Create(PersonFirstName, PersonLastName, PersonId));
            }

            _ = instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePerson).Should().BeFalse());
        }

        [Fact]
        public void CallingParameterlessCreateMultipleTimesProducesSameInstanceInScope()
        {
            PersonFactory.FactoryMode = FactoryMode.Singleton;
            var instances = new List<Person>();
            using IDisposable scope = PersonFactory.CreateScope();
            Person referencePerson = CreateSingletonReferenceDefaultPersonFromFactory();
            for (int count = 0; count < 10; count++)
            {
                instances.Add(PersonFactory.Create());
            }

            _ = instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePerson).Should().BeTrue());
        }

        [Fact]
        public void CallingParameterizedCreateMultipleTimesProducesSameInstanceInScope()
        {
            PersonFactory.FactoryMode = FactoryMode.Singleton;
            var instances = new List<Person>();
            using IDisposable scope = PersonFactory.CreateScope();
            Person referencePerson = CreateSingletonReferencePersonFromFactory();
            for (int count = 0; count < 10; count++)
            {
                instances.Add(PersonFactory.Create(PersonFirstName, PersonLastName, PersonId));
            }

            _ = instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePerson).Should().BeTrue());
        }

        [Fact]
        public void CallingParameterlessCreateProducedInstancesInScopeThatDifferFromSharedInstancesProducedOutsideOfScope()
        {
            PersonFactory.FactoryMode = FactoryMode.Singleton;
            Person referencePersonBeforeScope = PersonFactory.Create();
            var instances = new List<Person>();
            using (IDisposable scope = PersonFactory.CreateScope())
            {
                for (int count = 0; count < 10; count++)
                {
                    instances.Add(PersonFactory.Create());
                }
            }

            Person referencePersonAfterScope = PersonFactory.Create();

            _ = instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePersonBeforeScope).Should().BeFalse());
            _ = instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePersonAfterScope).Should().BeFalse());
        }

        [Fact]
        public void CallingParameterlessCreateProducesSameInstanceBeforeAndAfterScopeInSingletonMode()
        {
            PersonFactory.FactoryMode = FactoryMode.Singleton;
            Person referencePersonBeforeScope = PersonFactory.Create();
            var instances = new List<Person>();
            using (IDisposable scope = PersonFactory.CreateScope())
            {
                for (int count = 0; count < 10; count++)
                {
                    instances.Add(PersonFactory.Create());
                }
            }

            Person referencePersonAfterScope = PersonFactory.Create();
            _ = ReferenceEquals(referencePersonBeforeScope, referencePersonAfterScope).Should().BeTrue();
        }

        [Fact]
        public void CallingParameterizedCreateProducedInstancesInScopeThatDifferFromSharedInstancesProducedOutsideOfScope()
        {
            PersonFactory.FactoryMode = FactoryMode.Singleton;
            Person referencePersonBeforeScope = PersonFactory.Create(PersonFirstName, PersonLastName, PersonId);
            var instances = new List<Person>();
            using (IDisposable scope = PersonFactory.CreateScope())
            {
                for (int count = 0; count < 10; count++)
                {
                    instances.Add(PersonFactory.Create(PersonFirstName, PersonLastName, PersonId));
                }
            }

            Person referencePersonAfterScope = PersonFactory.Create(PersonFirstName, PersonLastName, PersonId);

            _ = instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePersonBeforeScope).Should().BeFalse());
            _ = instances.Should().AllSatisfy(person => ReferenceEquals(person, referencePersonAfterScope).Should().BeFalse());
        }

        [Fact]
        public void CallingParameterizedCreateProducesSameInstanceBeforeAndAfterScopInSingletonMode()
        {
            PersonFactory.FactoryMode = FactoryMode.Singleton;
            Person referencePersonBeforeScope = PersonFactory.Create(PersonFirstName, PersonLastName, PersonId);
            var instances = new List<Person>();
            using (IDisposable scope = PersonFactory.CreateScope())
            {
                for (int count = 0; count < 10; count++)
                {
                    instances.Add(PersonFactory.Create(PersonFirstName, PersonLastName, PersonId));
                }
            }

            Person referencePersonAfterScope = PersonFactory.Create(PersonFirstName, PersonLastName, PersonId);
            _ = ReferenceEquals(referencePersonBeforeScope, referencePersonAfterScope).Should().BeTrue();
        }

        [Fact]
        public void CallingParameterlessCreateProducesDifferentInstanceBeforeAndAfterScopInTransientMode()
        {
            PersonFactory.FactoryMode = FactoryMode.Transient;
            Person referencePersonBeforeScope = PersonFactory.Create();
            var instances = new List<Person>();
            using (IDisposable scope = PersonFactory.CreateScope())
            {
                for (int count = 0; count < 10; count++)
                {
                    instances.Add(PersonFactory.Create());
                }
            }

            Person referencePersonAfterScope = PersonFactory.Create();
            _ = ReferenceEquals(referencePersonBeforeScope, referencePersonAfterScope).Should().BeFalse();
        }

        [Fact]
        public void CallingParameterizedCreateProducesDifferentInstanceBeforeAndAfterScopInTransientMode()
        {
            PersonFactory.FactoryMode = FactoryMode.Transient;
            Person referencePersonBeforeScope = PersonFactory.Create(PersonFirstName, PersonLastName, PersonId);
            var instances = new List<Person>();
            using (IDisposable scope = PersonFactory.CreateScope())
            {
                for (int count = 0; count < 10; count++)
                {
                    instances.Add(PersonFactory.Create(PersonFirstName, PersonLastName, PersonId));
                }
            }

            Person referencePersonAfterScope = PersonFactory.Create(PersonFirstName, PersonLastName, PersonId);
            _ = ReferenceEquals(referencePersonBeforeScope, referencePersonAfterScope).Should().BeFalse();
        }

        [Fact]
        public void ChangingFactoryModeInScopeWillThrowException()
        {
            PersonFactory.FactoryMode = FactoryMode.Singleton;
            _ = Invoking(_this =>
            {
                var instances = new List<Person>();
                using IDisposable scope = _PersonFactory.CreateScope();
                PersonFactory.FactoryMode = FactoryMode.Transient;
                Person referencePerson = CreateSingletonReferencePersonFromFactory();
                for (int count = 0; count < 10; count++)
                {
                    instances.Add(_PersonFactory.Create(_PersonFirstName, _PersonLastName, _PersonId));
                }
            }).Should().ThrowExactly<InvalidOperationException>("changing the IFactory.FactoryMode property inside a scope is not allowed.");
        }

        private Person CreateReferencePerson() => new Person(PersonFirstName, PersonLastName, PersonId);

        private Person CreateSingletonReferencePersonFromFactory() => PersonFactory.Create(PersonFirstName, PersonLastName, PersonId);

        private Person CreateReferenceDefaultPerson() => new Person(PersonFactory.DefaultPersonFirstName, PersonFactory.DefaultPersonLastName, PersonFactory.DefaultPersonId);

        private Person CreateSingletonReferenceDefaultPersonFromFactory() => PersonFactory.Create(PersonFactory.DefaultPersonFirstName, PersonFactory.DefaultPersonLastName, PersonFactory.DefaultPersonId);

        public void Dispose()
        { }
    }
}
