using System;
using System.Windows.Markup;

namespace BionicCode.Utilities.Net.Framework.Wpf.Markup
{
  /// <summary>
  /// XAML extension to return the values of an enumeration.
  /// </summary>
  /// <example>Provide the enum type via constructor or by setting the <see cref="EnumType"/> property:
  /// <code><ComboBox ItemsSource="{Enum {x:Type MyEnum}}" /></code>
  /// <code><ComboBox ItemsSource="{Enum EnumType={x:Type MyEnum}}" /></code>
  /// </example>
  public class EnumExtension : MarkupExtension
  {
    /// <summary>
    /// The enum to enumerate.
    /// </summary>
    /// <value>The type of the enum to enumerate.</value>
    public Type EnumType { get; set; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public EnumExtension()
    {
    }

    /// <summary>
    /// Constructor to initialize the <see cref="EnumType"/> property.
    /// </summary>
    /// <param name="enumType"></param>
    public EnumExtension(Type enumType)
    {
      this.EnumType = enumType;
    }

    #region Overrides of MarkupExtension

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      if (this.EnumType == null)
      {
        throw new ArgumentException("The property 'EnumType' of markup extension 'EnumExtension' must be set.");
      }
      return Enum.GetValues(this.EnumType);
    }

    #endregion
  }
}
