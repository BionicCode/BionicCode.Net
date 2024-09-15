namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using System;
  using System.Runtime.CompilerServices;

  [System.AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
  sealed class TestAttributeInheritable : Attribute
  {
    // This is a positional argument
    public TestAttributeInheritable(double numericPositionalParameter, [CallerMemberName] string positionalString = null)
    {
      this.PositionalString = positionalString;
      this.TestClassPositionalParameter = numericPositionalParameter;

      // TODO: Implement code here

    }

    // This is a positional argument
    public TestAttributeInheritable(string positionalString)
    {
      this.PositionalString = positionalString;

      // TODO: Implement code here

    }
    public double TestClassPositionalParameter { get; }

    public string PositionalString { get; }

    // This is a named argument
    public int NamedInt { get; set; }
  }
}
