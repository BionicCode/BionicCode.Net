namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using System;

  public class TestClassWithInterfaceAndBaseClass<T> : TestClassBase<T, int>, ITestClass1<T, int, TestClassWithInterface>
    where T : class
  {
  }
}
