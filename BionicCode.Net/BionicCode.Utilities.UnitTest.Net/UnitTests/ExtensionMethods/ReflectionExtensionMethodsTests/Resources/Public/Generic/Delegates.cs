namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  public delegate void TestDelegateWithoutReturnValue<in T>(int a, T b, string text);
  public delegate TReturn TestDelegateWithReturnValue<T, out TReturn>(int a, T b, string text) where T : TestClassBase;
}
