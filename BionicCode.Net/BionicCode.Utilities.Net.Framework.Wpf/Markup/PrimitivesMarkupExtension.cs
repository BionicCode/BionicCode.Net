using System;
using System.Windows.Markup;

namespace BionicCode.Utilities.Net.Framework.Wpf.Markup
{
  /// <summary>
  /// XAML markup extension that allows to define primitive type values like <see cref="bool"/> or <see cref="int"/>.
  /// </summary>
  /// <remarks>Only allows a single value. In case multiple properties are set, the last set value will be used, as every value overrides the previously set.</remarks>
  public class PrimitiveExtension : MarkupExtension
  {
    private object primitiveValue;
    /// <summary>
    /// Holds a <see cref="bool"/>.
    /// </summary>
    /// <value>A <see cref="bool"/>.</value>
    public bool Boolean { set => this.primitiveValue = value; }
    /// <summary>
    /// Holds a <see cref="int"/>.
    /// </summary>
    /// <value>A <see cref="int"/>.</value>
    public int Int32 { set => this.primitiveValue = value; }
    /// <summary>
    /// Holds a <see cref="double"/>.
    /// </summary>
    /// <value>A <see cref="double"/>.</value>
    public double Double { set => this.primitiveValue = value; }
    /// <summary>
    /// Holds a <see cref="string"/>.
    /// </summary>
    /// <value>A <see cref="string"/>.</value>
    public string String { set => this.primitiveValue = value; }

    #region constructors

    /// <summary>
    /// Initializes a new instance of <seealso cref="PrimitiveExtension"/>.
    /// </summary>
    public PrimitiveExtension()
    {
      this.primitiveValue = null;
    }

    /// <summary>
    /// Initializes a new instance of <seealso cref="PrimitiveExtension"/>.
    /// </summary>
    /// <param name="booleanValue">The value of type <see cref="bool"/>.</param>
    public PrimitiveExtension(bool booleanValue)
    {
      this.primitiveValue = booleanValue;
    }

    /// <summary>
    /// Initializes a new instance of <seealso cref="PrimitiveExtension"/>.
    /// </summary>
    /// <param name="doubleValue">The value of type <see cref="double"/>.</param>
    public PrimitiveExtension(double doubleValue)
    {
      this.primitiveValue = doubleValue;
    }

    /// <summary>
    /// Initializes a new instance of <seealso cref="PrimitiveExtension"/>.
    /// </summary>
    /// <param name="stringValue">The value of type <see cref="string"/>.</param>
    public PrimitiveExtension(string stringValue)
    {
      this.primitiveValue = stringValue;
    }

    /// <summary>
    /// Initializes a new instance of <seealso cref="PrimitiveExtension"/>.
    /// </summary>
    /// <param name="int32Value">The value of type <see cref="int"/>.</param>
    public PrimitiveExtension(int int32Value)
    {
      this.primitiveValue = int32Value;
    }


    #endregion

    #region Overrides of MarkupExtension

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) => this.primitiveValue;

    #endregion
  }
}
