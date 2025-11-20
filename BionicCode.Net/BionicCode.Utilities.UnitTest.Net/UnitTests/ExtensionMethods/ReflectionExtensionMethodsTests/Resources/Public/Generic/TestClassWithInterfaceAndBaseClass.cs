namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
    public class TestClassWithInterfaceAndBaseClass<T> : TestClassBase<T, int>, ITestClass1<T, int, TestClassWithInterface>
      where T : class
    {
    }
}
