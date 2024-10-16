namespace BionicCode.Utilities.Net.UnitTest.ExtensionMethodsTests
{ 
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using FluentAssertions;
  using Xunit;

  public class EnumToStringExtensionMethodsTests
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
