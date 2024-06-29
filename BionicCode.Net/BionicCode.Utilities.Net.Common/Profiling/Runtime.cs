namespace BionicCode.Utilities.Net
{
  /// <summary>
  /// Used to specify the environment that the <see cref="Profiler"/> should use to profile the code.
  /// </summary>
  public enum Runtime
  {
    Default = 0,
    Current,
    NetFramework4_7_2,
    NetFramework4_8,
    NetFramework4_8_1,
    NetStandard2_0,
    NetStandard2_1,
    NetCore2_0,
    NetCore2_1,
    NetCore2_2,
    NetCore3_0,
    NetCore3_1,
    Net5_0,
    Net6_0,
    Net7_0,
    Net8_0,
  }
}
