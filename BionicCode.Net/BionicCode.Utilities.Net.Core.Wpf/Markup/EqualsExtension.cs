using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace BionicCode.Utilities.Net.Core.Wpf.Markup
{
  /// <summary>
  /// XAML Equality comparer.
  /// </summary>
  /// <example>Provide the enum type via constructor or by setting the <see cref="EnumType"/> property:<para></para><code>&lt;Trigger Property="Count" Value="{Equals /&gt;</code>
  /// <para></para>
  /// <code><ComboBox ItemsSource="{Enum EnumType={x:Type MyEnum}}" /></code></example>
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

      public override object ProvideValue(IServiceProvider serviceProvider)
      {
        return this.valueY == null ? this.valueX?.GetType().Equals(this.typeToEqual) ?? false : this.valueX?.Equals(this.valueY) ?? false;
      }
    }
}
