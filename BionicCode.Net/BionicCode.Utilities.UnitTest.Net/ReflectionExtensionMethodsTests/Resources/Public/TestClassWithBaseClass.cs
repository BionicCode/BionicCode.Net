namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic;

  [TestAttribute(1024.25, "class", NamedInt = 128)]
  [TestAttribute(64.0, "class", NamedInt = 256)]
  public class TestClassWithBaseClass : TestClassBase
  {
    public override event EventHandler TestEvent;
  }
}
