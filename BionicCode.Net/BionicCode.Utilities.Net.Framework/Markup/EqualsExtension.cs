using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace BionicCode.Utilities.Net.Framework.Markup
{
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
