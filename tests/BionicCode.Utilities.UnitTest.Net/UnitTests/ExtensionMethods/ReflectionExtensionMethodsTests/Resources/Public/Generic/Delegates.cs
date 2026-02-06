namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
    public delegate void TestDelegateWithoutReturnValue<in T>(int a, T b, string text);
    public delegate TReturn TestDelegateWithReturnValue<T, out TReturn>(int a, T b, string text) where T : TestClassBase;
}
