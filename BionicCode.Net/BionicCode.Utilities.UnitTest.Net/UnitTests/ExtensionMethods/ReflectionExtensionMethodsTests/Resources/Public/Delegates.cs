namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  public delegate void TestDelegateWithoutReturnValue(int a, TestClass b, string text);
  public delegate int TestDelegateWithReturnValue(int a, TestClass b, string text);
}
