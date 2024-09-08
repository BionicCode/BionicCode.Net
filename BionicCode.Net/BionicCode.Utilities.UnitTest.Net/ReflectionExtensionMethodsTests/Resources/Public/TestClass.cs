namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  public class TestClass
  {
    private readonly string readOnlyField;
    private string field;
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
