namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public
{
    using System;
    using BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic;

    [TestAttribute(1024.25, "class", NamedInt = 128)]
    [TestAttribute(64.9, "class", NamedInt = 256)]
    public class TestClassWithBaseClass : TestClassBase
    {
        public override event EventHandler TestEvent;
    }
}
