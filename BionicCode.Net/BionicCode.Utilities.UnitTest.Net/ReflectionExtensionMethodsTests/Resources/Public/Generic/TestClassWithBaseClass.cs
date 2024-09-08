namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  public class TestClassWithBaseClass<T, U> : TestClassBase<T, U>
    where T : class
    where U : struct
  {
  }
}
