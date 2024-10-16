namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public;

  public static class TestClassStatic<T, U, I, V> where T : class where U : struct where I : ITestClass1 where V : new()
  {
  }
}
