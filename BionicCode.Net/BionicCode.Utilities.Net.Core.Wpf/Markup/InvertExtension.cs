using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using BionicCode.Utilities.Net.Standard;

namespace BionicCode.Utilities.Net.Core.Wpf.Markup
{
  public class InvertExtension : MarkupExtension
  {
    public object Value { get; set; }
    public InversionMode Mode { get; set; }
    private IValueInverter ValueInverter { get; set; }

    public InvertExtension() : this(DependencyProperty.UnsetValue)
    {
    }

    public InvertExtension(object value)
    {
      this.Value = value;
      this.ValueInverter = new DefaultValueInverter();
      this.Mode = InversionMode.Default;
    }

    #region Overrides of MarkupExtension

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      object valueToInvert;
      if (this.Value is MarkupExtension innerMarkupExtension)
      {
        valueToInvert = GetValueToInvertFromMarkupExtension(innerMarkupExtension, serviceProvider);
        if (valueToInvert is BindingBase bindingMarkupExtension)
        {
          return bindingMarkupExtension.ProvideValue(serviceProvider);
        }
      }
      else
      {
        valueToInvert = this.Value;
      }

      var provideValueTargetService = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
      Type targetPropertyType = (provideValueTargetService.TargetProperty as DependencyProperty).PropertyType;

      return valueToInvert == DependencyProperty.UnsetValue
        ? valueToInvert
        : this.ValueInverter.TryInvertValue(valueToInvert, out object invertedValue)
          ? targetPropertyType.Equals(typeof(string))
            ? invertedValue.ToString()
            : invertedValue
          : valueToInvert;
    }
    #endregion

    protected object GetValueToInvertFromMarkupExtension(MarkupExtension wrappedMarkupExtension,
      IServiceProvider serviceProvider)
    {
      if (wrappedMarkupExtension is Binding bindingExpression)
      {
        return GetValueFomBinding(serviceProvider, bindingExpression);
      }

      return wrappedMarkupExtension.ProvideValue(serviceProvider);
    }

    private object GetValueFomBinding(IServiceProvider serviceProvider, Binding bindingExpression)
    {
      var provideValueTargetService = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
      object targetObject = provideValueTargetService?.TargetObject;
      if (targetObject == null)
      {
        return this;
      }

      var bindingResolver = new BindingResolver(
        targetObject as FrameworkElement,
        provideValueTargetService.TargetProperty as DependencyProperty)
      {
        InversionMode = this.Mode,
        ValueInverter = this.ValueInverter
      };

      return bindingResolver.ResolveBinding(bindingExpression, this.Mode);
    }
  }
}