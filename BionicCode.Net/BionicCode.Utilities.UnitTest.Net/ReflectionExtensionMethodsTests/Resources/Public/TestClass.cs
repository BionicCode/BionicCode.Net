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
    private int field;
    public virtual event EventHandler Event;

    public bool HasValue
    {
      get => true;
    }

    public TestClass(int parameter)
    { }

    public int PublicMethodWithReturnValue(string parameter)
    {
      return 0;
    }

    public ref readonly int PublicMethodWithReadOnlyRefReturnValue(ref int parameter)
    {
      ref int value = ref field;
      return ref field;
    }

    public TValue PublicGenericMethodWithReturnValue<TValue>(TValue parameter)
    {
      return default;
    }
  }

  public readonly struct TestStruct
  {

#if NET
    public readonly bool HasValue
    {
      get => true;
    }
#endif
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
