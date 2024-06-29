
namespace BionicCode.Utilities.Net
{
  using System;
#if NET || NET461_OR_GREATER
  using System.Windows.Data;
  using System.Windows.Markup;

  /// <summary>
  /// XAML Equality comparer.
  /// </summary>
  public class EqualsExtension : MarkupExtension
  {
    private readonly Type typeToEqual;
    private readonly object valueX;
    private readonly object valueY;

    public EqualsExtension(object valueXToCompare, Type expectedType)
    {
      if (valueXToCompare is Binding)
      {
        this.valueX = (valueXToCompare as Binding).Path;
      }
      else
      {
        this.valueX = valueXToCompare;
      }

      this.typeToEqual = expectedType;
      this.valueY = null;
    }

    public EqualsExtension(object valueX, object valueY)
    {
      this.valueX = valueX;
      this.valueY = valueY;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this.valueY == null ? this.valueX?.GetType().Equals(this.typeToEqual) ?? false : this.valueX?.Equals(this.valueY) ?? false;
  }
#endif
}
