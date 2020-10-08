using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace BionicCode.Utilities.Net.Framework.Wpf.Markup
{
  public class EqualsExtension : MarkupExtension
  {
      private readonly Type typeToEqual;
      public object ValueX { get; set; }
      public object ValueY { get; set; }

      public EqualsExtension(object valueXToCompare, Type expectedType)
      {
        if (valueXToCompare is Binding)
        {
          this.ValueX = (valueXToCompare as Binding).Path;
        }
        else
        {
          this.ValueX = valueXToCompare;
        }

        this.typeToEqual = expectedType;
        this.ValueY = null;
      }

      public EqualsExtension(object valueX, object valueY)
      {
        this.ValueX = valueX;
        this.ValueY = valueY;
      }

      public override object ProvideValue(IServiceProvider serviceProvider)
      {
        return this.ValueY == null ? this.ValueX?.GetType().Equals(this.typeToEqual) ?? false : this.ValueX?.Equals(this.ValueY) ?? false;

        //bool isEqual;
        //if (this.ValueX is MarkupExtension innerMarkupExtension)
        //{
        //  isEqual = GetValueToInvertFromMarkupExtension(innerMarkupExtension, serviceProvider);
        //  if (isEqual is BindingBase bindingMarkupExtension)
        //  {
        //    return bindingMarkupExtension.ProvideValue(serviceProvider);
        //  }
        //}
        //else
        //{
        //  isEqual = this.Value;
        //}

        //var provideValueTargetService = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        //Type targetPropertyType = (provideValueTargetService.TargetProperty as DependencyProperty).PropertyType;

        //return isEqual == DependencyProperty.UnsetValue
        //  ? isEqual
        //  : this.ValueInverter.TryInvertValue(isEqual, out object invertedValue)
        //    ? targetPropertyType.Equals(typeof(string))
        //      ? invertedValue.ToString()
        //      : invertedValue
        //    : isEqual;
    }

      //protected object GetValueToInvertFromMarkupExtension(MarkupExtension wrappedMarkupExtension,
      //  IServiceProvider serviceProvider)
      //{
      //  if (wrappedMarkupExtension is Binding bindingExpression)
      //  {
      //    return GetValueFomBinding(serviceProvider, bindingExpression);
      //  }

      //  return wrappedMarkupExtension.ProvideValue(serviceProvider);
      //}

      //private object GetValueFomBinding(IServiceProvider serviceProvider, Binding bindingExpression)
      //{
      //  var provideValueTargetService = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
      //  object targetObject = provideValueTargetService?.TargetObject;
      //  if (targetObject == null)
      //  {
      //    return this;
      //  }

      //  var bindingResolver = new BindingResolver(
      //    targetObject as FrameworkElement,
      //    provideValueTargetService.TargetProperty as DependencyProperty)
      //  {
      //    InversionMode = this.Mode,
      //    ResolvedValueFilter = this.ValueInverter.InvertValue
      //  };

      //  return bindingResolver.ResolveBinding(bindingExpression, this.Mode);
      //}
  }
}
