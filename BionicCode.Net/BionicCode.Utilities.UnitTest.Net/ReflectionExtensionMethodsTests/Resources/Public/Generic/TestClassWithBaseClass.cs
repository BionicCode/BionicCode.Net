namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Runtime.CompilerServices;
  using System.Threading.Tasks;
  using Microsoft.CodeAnalysis;

  [TestAttribute(1024.25, "class", NamedInt = 128)]
  [TestAttribute(64.0, "class", NamedInt = 256)]
  public class TestClassWithBaseClass<T, U> : TestClassBase<T, U>
    where T : class
    where U : struct
  {
    [TestAttribute(1024.25, "method", NamedInt = 128)]
    [TestAttribute(64.0, "method", NamedInt = 256)]
    public T PublicGenericMethodWithReturnValue<V, W>([TestAttribute(12.0, "parameter", NamedInt = 24)] ref V parameter, W parameter2, out IEnumerable<int> parameter3, in IDictionary<int, string> parameter4) where V : class, IList, ITestClass1, ITestClass3, Generic.ITestClass2<T, U>, new() where W : struct, IComparable, ITestClass2
    {
      parameter3 = null;
      return default;
    }

    [TestAttribute(1024.25, "method", NamedInt = 128)]
    [TestAttribute(64.0, "method", NamedInt = 256)]
    public async Task<T> PublicGenericMethodWithReturnValueAsync<V, W>([TestAttribute(12.0, "parameter", NamedInt = 24)] V parameter, W parameter2, IEnumerable<int> parameter3, IDictionary<int, string> parameter4) where V : class, IList, ITestClass1, ITestClass3, Generic.ITestClass2<T, U>, new() where W : struct, IComparable, ITestClass2
    {
      return default;
    }
  }
}
