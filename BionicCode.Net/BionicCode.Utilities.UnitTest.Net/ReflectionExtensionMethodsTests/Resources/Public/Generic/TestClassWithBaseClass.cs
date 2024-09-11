namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using System;
  using System.Collections;
  using Microsoft.CodeAnalysis;

  public class TestClassWithBaseClass<T, U> : TestClassBase<T, U>
    where T : class
    where U : struct
  {

    public T PublicGenericMethodWithReturnValue<V, W>(V parameter, W parameter2) where V : IList, new() where W : struct, IComparable
    {
      return default;
    }
  }
}
