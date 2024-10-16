namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  public class TestClassWithBaseClasses<T> : TestClassWithBaseClass
   where T : class
  {
  }
}
