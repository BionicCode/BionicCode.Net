namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public;

  public class TestClassWithInterfaces<T, U, I> : ITestClass1<T, U, I>, ITestClass2<T, U>, ITestClass3
    where T : class
    where U : struct
    where I : ITestClass1
  {
  }
}
