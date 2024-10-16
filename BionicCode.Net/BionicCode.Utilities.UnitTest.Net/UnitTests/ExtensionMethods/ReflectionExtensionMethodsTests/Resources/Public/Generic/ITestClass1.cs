namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  public interface ITestClass1<T, U, I>
    where T : class
    where U : struct
    where I : ITestClass1
  { }
}
