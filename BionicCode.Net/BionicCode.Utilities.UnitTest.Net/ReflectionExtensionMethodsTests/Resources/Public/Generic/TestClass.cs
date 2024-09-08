﻿namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public;

  public class TestClass<T, U, I, V> where T : class where U : struct where I : ITestClass1 where V : new()
  {
    private readonly string readOnlyField;
    private string field;

    public TestClass(int parameter)
    { }

    public int PublicMethodWithReturnValue(string parameter)
    {
      return 0;
    }

    public U PublicGenericMethodWithReturnValue(T parameter)
    {
      return default;
    }
  }
}
