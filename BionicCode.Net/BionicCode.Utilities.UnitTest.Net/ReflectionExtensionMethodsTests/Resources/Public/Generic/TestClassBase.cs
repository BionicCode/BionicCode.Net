namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  [TestAttribute(1024.25, "class", NamedInt = 128)]
  [TestAttribute(64.0, "class", NamedInt = 256)]
  [TestAttributeInheritable(64.0, "class", NamedInt = 256)]
  [TestAttributeInheritable(64.0, "class", NamedInt = 256)]
  public abstract class TestClassBase<T, U>
    where T : class
    where U : struct
  {
  }
}
