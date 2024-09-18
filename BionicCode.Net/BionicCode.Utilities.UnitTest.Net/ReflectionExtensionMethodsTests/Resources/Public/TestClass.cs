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

    public TestClass(in int parameter)
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

  public ref struct TestRefStruct
  {
    public const int Constant = 1;
#if NET7_0_OR_GREATER
    public readonly ref int readonlyRefInteger;
    public ref int refInteger;
    public readonly bool InitOnlyBoolean { get; init; }
    public bool ReadOnlySetBoolean { get => true; private readonly set => _ = value; }
    public readonly bool ReadOnlyBoolean { get; }

    public readonly bool HasValue
    {
      get => true;
    }

    public ref readonly int GetNumber()
    {
      int i = 9;
      ref int j = ref i;
      return ref this.refInteger;
    }
#endif
    public int integer;
    public readonly int readonlyInteger;
    public bool Boolean { get; }

    public void GetNumber2()
    {
    }
  }
}

