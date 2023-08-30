namespace BionicCode.Utilities.Net.UnitTest
{
  using Xunit;
  using FluentAssertions;
  using BionicCode.Utilities.Net;
  using System;

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
