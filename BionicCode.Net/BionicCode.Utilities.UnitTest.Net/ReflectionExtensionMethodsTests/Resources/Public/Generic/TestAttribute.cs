namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic
{
  using System;

  [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
  sealed class TestAttribute : Attribute
  {
    // This is a positional argument
    public TestAttribute(string positionalString, double numericPositionalParameter)
    {
      this.PositionalString = positionalString;
      this.TestClassPositionalParameter = numericPositionalParameter;

      // TODO: Implement code here

    }

    // This is a positional argument
    public TestAttribute(string positionalString)
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
