namespace BionicCode.Utilities.Net
{
  using System.Runtime.CompilerServices;

  public static class ReflectionHelper
  {
    public static string GetCurrentMemberName([CallerMemberName] string callerMemberName = null) => callerMemberName;
  }
}
