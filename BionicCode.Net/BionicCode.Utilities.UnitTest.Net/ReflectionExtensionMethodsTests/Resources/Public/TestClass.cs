namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic;

  [TestAttribute(1024.25, "class", NamedInt = 128)]
  [TestAttribute(64.0, "class", NamedInt = 256)]
  public class TestClass
  {
    private readonly string readOnlyField;
    private string field;
    public virtual event EventHandler Event;

    public TestClass(int parameter)
    { }

    public int PublicMethodWithReturnValue(string parameter)
    {
      return 0;
    }

    public TValue PublicGenericMethodWithReturnValue<TValue>(TValue parameter)
    {
      return default;
    }
  }

  public struct TestStruct
  {
  }

  public readonly struct TestReadOnlyStruct
  {
  }

  public enum TestEnum
  {
    Default = 0,
    TestValue,
  }

}
