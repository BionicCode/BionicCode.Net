namespace BionicCode.Utilities.Net.UnitTest.ProfilerTests
{
  using System;
  using BionicCode.Utilities.Net;
  using FluentAssertions;
  using Xunit;

  public class ProfiledTargetTypeTest
  {
    [Fact]
    public void ExtensionMethod_ToDisplayStringValue_ShouldNotThrowBecauseAllValuesAreSupported()
    {
      Action act = () =>
      {
        foreach (ProfiledTargetType profiledTargetType in Enum.GetValues(typeof(ProfiledTargetType)))
        {
          _ = profiledTargetType.ToDisplayStringValue();
        }
      };

      _ = act.Should().NotThrow<NotSupportedException>("all values are supported.");
    }
  }
}
