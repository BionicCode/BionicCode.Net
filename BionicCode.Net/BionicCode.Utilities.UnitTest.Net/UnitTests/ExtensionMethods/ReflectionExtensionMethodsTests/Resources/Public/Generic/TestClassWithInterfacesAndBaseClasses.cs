namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using System;

  public class TestClassWithInterfacesAndBaseClasses<T> : TestClassWithInterfaceAndBaseClass<T>, ITestClass1<T, int, TestClassWithInterface>
    where T : class
  {
  }
}
