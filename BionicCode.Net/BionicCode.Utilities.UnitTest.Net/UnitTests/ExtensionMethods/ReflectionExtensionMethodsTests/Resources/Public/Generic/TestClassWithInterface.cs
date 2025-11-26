namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
    using System.ComponentModel.DataAnnotations;

    public class TestClassWithInterface<T> : ITestClass1<T, int, TestClassWithInterface>
      where T : class
    {
        [Required(ErrorMessage = "MyProperty is required.")]
        public T MyProperty { get; set; }
    }
}
