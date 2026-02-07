
namespace BionicCode.Utilities.Net
{
    using System;
#if !NETSTANDARD
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
                valueX = (valueXToCompare as Binding).Path;
            }
            else
            {
                valueX = valueXToCompare;
            }

            typeToEqual = expectedType;
            valueY = null;
        }

        public EqualsExtension(object valueX, object valueY)
        {
            valueX = valueX;
            valueY = valueY;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => valueY == null ? valueX?.GetType().Equals(typeToEqual) ?? false : valueX?.Equals(valueY) ?? false;
    }
#endif
}
