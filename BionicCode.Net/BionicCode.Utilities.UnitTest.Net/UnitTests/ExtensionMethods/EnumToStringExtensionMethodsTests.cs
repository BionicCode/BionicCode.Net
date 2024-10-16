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
    public void ExtensionMethod_ProfiledTargetTypeToDisplayStringValue_ShouldNotThrowBecauseAllValuesAreSupported()
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

    [Fact]
    public void ExtensionMethod_AccessModifierToDisplayStringValue_ShouldNotThrowBecauseAllValuesAreSupported()
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

    [Fact]
    public void ExtensionMethod_TimeUnitToDisplayStringValue_ShouldNotThrowBecauseAllValuesAreSupported()
    {
      Action act = () =>
      {
        foreach (TimeUnit timeUnit in Enum.GetValues(typeof(TimeUnit)))
        {
          _ = timeUnit.ToDisplayStringValue();
        }
      };

      _ = act.Should().NotThrow<NotSupportedException>("all values are supported.");
    }

    [Fact]
    public void ExtensionMethod_SymbolAttributesToDisplayStringValue_ShouldNotThrowBecauseAllValuesAreSupported()
    {
      Action act = () =>
      {
        foreach (SymbolAttributes symbolAttributes in Enum.GetValues(typeof(SymbolAttributes)))
        {
          _ = symbolAttributes.ToDisplayTypeKind();
        }
      };

      _ = act.Should().NotThrow<NotSupportedException>("all values are supported.");
    }
  }
}
