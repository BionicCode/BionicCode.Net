namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using System;
  using System.Collections;
  using System.Runtime.CompilerServices;
  using Microsoft.CodeAnalysis;

  public class TestClassWithBaseClass<T, U> : TestClassBase<T, U>
    where T : class
    where U : struct
  {
    [TestAttribute("method", 1024.0, NamedInt = 32)]
    public T PublicGenericMethodWithReturnValue<V, W>([TestAttribute("parameter", 12.0, NamedInt = 24)] ref V parameter, W parameter2) where V : class, IList, new() where W : struct, IComparable
    {
      return default;
    }
  }
}
