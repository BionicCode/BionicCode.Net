namespace BionicCode.Utilities.Net.UnitTest
{
  using System;
  using BionicCode.Utilities.Net;
  using FluentAssertions;
  using Xunit;

  public class AccessModifierTest
  {
    [Fact]
    public void ExtensionMethod_ToDisplayStringValue_ShouldNotThrowBecauseAllValuesAreSupported()
    {
      Action act = () =>
      {
        foreach (AccessModifier accessModifier in Enum.GetValues(typeof(AccessModifier)))
        {
          _ = accessModifier.ToDisplayStringValue();
        }
      };

      _ = act.Should().NotThrow<NotSupportedException>("all values are supported.");
    }
  }
}
