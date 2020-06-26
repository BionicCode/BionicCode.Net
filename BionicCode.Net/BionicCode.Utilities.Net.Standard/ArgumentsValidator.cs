using System.Linq;

namespace BionicCode.Utilities.Net.Standard
{
  public class ArgumentsValidator
  {
    public static bool ArgsAreNull(params object[] argsToValidate)
    {
      return argsToValidate.Any((arg) => arg == null);
    }
  }
}
