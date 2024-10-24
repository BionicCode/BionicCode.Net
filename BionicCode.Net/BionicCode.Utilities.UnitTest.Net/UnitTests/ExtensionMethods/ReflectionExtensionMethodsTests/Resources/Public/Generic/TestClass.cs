﻿namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using System.Text.Json.Serialization;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public;

  [TestAttribute(1024.25, "class", NamedInt = 128)]
  [TestAttribute(64.0, "class", NamedInt = 256)]
  public class TestClass<T, U, I, V> 
    where T : class 
    where U : struct
    where I : ITestClass1 
    where V : new()
  {
    private readonly string readOnlyField;
    private string field;

    public string this[int index]
    { get
      {
        return string.Empty;
      }
      set
      { 
      }
    }

    public TestClass()
    { }

    public TestClass(int parameter)
    { }

    public void PublicVoidMethod(string parameter)
    {
    }

    public int PublicMethodWithReturnValue(string parameter)
    {
      return 0;
    }

    public U PublicGenericMethodWithReturnValue(T parameter)
    {
      return default;
    }

    public async Task<U> PublicGenericAsyncMethodWithReturnValue(T parameter)
    {
      return await Task.FromResult<U>(default);
    }
  }
}