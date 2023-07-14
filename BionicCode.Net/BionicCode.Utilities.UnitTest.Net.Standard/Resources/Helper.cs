namespace BionicCode.Utilities.UnitTest.Net.Resources
{
  internal static class UnitTestHelper
  {
    internal static bool IsDebugModeEnabled =>
#if DEBUG
        true;
#else
        false;
#endif

  }
}
