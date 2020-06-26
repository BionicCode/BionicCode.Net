using System;
using System.Windows.Markup;

namespace BionicCode.Utilities.Net.Core.Wpf.Markup
{
  public class PrimitiveTypeExtension : MarkupExtension
  {
    private object primitiveValue;
    public bool Boolean { set { this.primitiveValue = value; } }
    public int Int32 { set { this.primitiveValue = value; } }
    public double Double { set { this.primitiveValue = value; } }
    public string String { set { this.primitiveValue = value; } }

    public PrimitiveTypeExtension()
    {
      this.primitiveValue = null;
    }

    #region constructors

    public PrimitiveTypeExtension(bool booleanValue)
    {
      this.primitiveValue = booleanValue;
    }

    public PrimitiveTypeExtension(double doubleValue)
    {
      this.primitiveValue = doubleValue;
    }

    public PrimitiveTypeExtension(string stringValue)
    {
      this.primitiveValue = stringValue;
    }

    public PrimitiveTypeExtension(int int32Value)
    {
      this.primitiveValue = int32Value;
    }


    #endregion

    #region Overrides of MarkupExtension

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this.primitiveValue;
    }

    #endregion
  }
}
