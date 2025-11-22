namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public
{
    using System;
    using System.Threading.Tasks;
    using BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic;

    [TestAttribute(1024.25, "class", NamedInt = 128)]
    [TestAttribute(64.0, "class", NamedInt = 256)]
    public class TestClass
    {
        private readonly string readOnlyField;
        private int field;
        public virtual event EventHandler Event;

        public bool HasValue => true;

        public TestClass(in int parameter)
        { }

        public async Task<int> PublicMethodWithReturnValue(string parameter)
        {
            Task result = await Task.WhenAny(PublicMethodWithReturnValue("hello"), new Task(() => { }));
            return 0;
        }

        public ref readonly int PublicMethodWithReadOnlyRefReturnValue(ref int parameter)
        {
            ref int value = ref this.field;
            return ref this.field;
        }

        public TValue PublicGenericMethodWithReturnValue<TValue>(TValue parameter) => default;
    }

    public struct TestStruct
    {

#if NET
        public readonly bool HasValue => true;
#endif
    }

    public readonly struct TestReadOnlyStruct
    {
    }

    public enum TestEnum
    {
        Default = 0,
        TestValue,
    }

    public ref struct TestRefStruct
    {
        public const int Constant = 1;
#if NET7_0_OR_GREATER
        public readonly ref int readonlyRefInteger;
        public ref int refInteger;
        public readonly bool InitOnlyBoolean { get; init; }
        public bool ReadOnlySetBoolean { get => true; private readonly set => _ = value; }
        public readonly bool ReadOnlyBoolean { get; }

        public readonly bool HasValue => true;

        public ref readonly int GetNumber()
        {
            int i = 9;
            ref int j = ref i;
            return ref this.refInteger;
        }
#endif
        public int integer;
        public readonly int readonlyInteger;
        public bool Boolean { get; }

        public void GetNumber2()
        {
        }
    }
}
