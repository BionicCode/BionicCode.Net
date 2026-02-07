namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
    public class TestClassWithInterface<T> : ITestClass1<T, int, TestClassWithInterface>
      where T : class
    {
        public T MyProperty { get; set; }
    }
}
